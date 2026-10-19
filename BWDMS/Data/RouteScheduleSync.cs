using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BWDMS.Data
{
    // =============================================================
    // ROUTE  ->  ROUTE-DAY SYNC
    //
    // A Route is the single source of truth. It already carries the
    // days it runs on (OrderDispatchDays) plus the vehicle, the
    // salesman and the driver selected in Add Route.
    //
    // Everything the rest of the application is keyed on -- Orders,
    // Dispatches, VehicleStock, Reconciliation, Billing and the
    // salesman app all store RouteScheduleId -- is DERIVED from that
    // definition here. That is why there is no "Route Schedules"
    // screen any more: saving a Route produces its route-day rows.
    //
    // RouteSchedules only ever stores a day NAME (Monday, Tuesday,
    // ...) and never a date, so the same row is simply revisited
    // every week. The per-date StockDate / IsClosed columns are what
    // close a day for one week and open it again for the next --
    // nothing has to be recreated each week.
    // =============================================================

    public static class RouteScheduleSync
    {
        // ---------------------------------------------------------
        // Make the route-days of ONE route match the Route exactly.
        //
        //  * a ticked day that has no row yet      -> inserted
        //  * a ticked day that already has a row   -> vehicle,
        //    salesman, driver and status refreshed
        //  * a row for a day that is no longer
        //    ticked                                -> switched off
        //
        // The route's villages are then mirrored onto every one of
        // its route-days, so the visit order defined under
        // Route Villages follows the route automatically.
        // ---------------------------------------------------------

        public static void SyncRoute(
            SqlConnection con,
            SqlTransaction transaction,
            int routeId,
            int userId,
            IList<string> days,
            object vehicleId,
            object salesmanId,
            object driverId,
            bool isActive)
        {
            if (con == null)
            {
                throw new ArgumentNullException("con");
            }

            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }


            HashSet<string> wanted = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            if (days != null)
            {
                foreach (string day in days)
                {
                    if (!string.IsNullOrWhiteSpace(day))
                    {
                        wanted.Add(day.Trim());
                    }
                }
            }


            // ----------------------------------------------------
            // What the route already has
            // ----------------------------------------------------

            List<KeyValuePair<string, int>> current =
                new List<KeyValuePair<string, int>>();

            const string selectQuery = @"
                SELECT RouteScheduleId, DayOfWeek
                FROM RouteSchedules
                WHERE RouteId = @RouteId";

            using (SqlCommand cmd =
                new SqlCommand(selectQuery, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        current.Add(
                            new KeyValuePair<string, int>(
                                Convert.ToString(
                                    reader["DayOfWeek"]),
                                Convert.ToInt32(
                                    reader["RouteScheduleId"])));
                    }
                }
            }


            HashSet<int> handled = new HashSet<int>();


            // ----------------------------------------------------
            // 1. Every ticked day exists, with this route's
            //    vehicle / salesman / driver on it
            // ----------------------------------------------------

            foreach (string day in wanted)
            {
                int scheduleId = 0;
                bool found = false;

                foreach (KeyValuePair<string, int> pair in current)
                {
                    if (found)
                    {
                        break;
                    }

                    if (string.Equals(
                        pair.Key,
                        day,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        scheduleId = pair.Value;
                        found = true;
                    }
                }

                if (found)
                {
                    handled.Add(scheduleId);

                    UpdateRouteDay(
                        con, transaction, scheduleId,
                        vehicleId, salesmanId, driverId,
                        isActive, userId);
                }
                else
                {
                    int newId = InsertRouteDay(
                        con, transaction, routeId, day,
                        vehicleId, salesmanId, driverId,
                        isActive, userId);

                    handled.Add(newId);
                }
            }


            // ----------------------------------------------------
            // 2. A day that is no longer ticked is switched off,
            //    never deleted -- orders already taken for it stay
            //    valid and billable.
            // ----------------------------------------------------

            foreach (KeyValuePair<string, int> pair in current)
            {
                if (handled.Contains(pair.Value))
                {
                    continue;
                }

                DeactivateRouteDay(
                    con, transaction, pair.Value, userId);
            }


            // ----------------------------------------------------
            // 3. Villages / visit order for every route-day
            // ----------------------------------------------------

            SyncVillageVisits(con, transaction, routeId, userId);
        }


        // ---------------------------------------------------------
        // Mirror the route's villages (and their visit order) onto
        // every route-day of that route.
        //
        // Called when a route is saved and when the village
        // assignment of a route changes, so no per-day village
        // screen is needed either.
        // ---------------------------------------------------------

        public static void SyncVillageVisits(
            SqlConnection con,
            SqlTransaction transaction,
            int routeId,
            int userId)
        {
            if (con == null)
            {
                throw new ArgumentNullException("con");
            }

            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }


            // a. a village that left the route takes its visits with it
            const string deleteQuery = @"
                DELETE rvs
                FROM RouteVillageSchedules rvs
                INNER JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = rvs.RouteScheduleId
                WHERE rs.RouteId = @RouteId
                  AND NOT EXISTS
                (
                    SELECT 1
                    FROM RouteVillages rv
                    WHERE rv.RouteVillageId = rvs.RouteVillageId
                      AND rv.IsActive = 1
                )";

            using (SqlCommand cmd =
                new SqlCommand(deleteQuery, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                cmd.ExecuteNonQuery();
            }


            // b. every active village becomes a visit of every
            //    route-day of this route, in the route's own order
            const string insertQuery = @"
                INSERT INTO RouteVillageSchedules
                (
                    RouteScheduleId,
                    RouteVillageId,
                    VisitSequence,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                SELECT
                    rs.RouteScheduleId,
                    rv.RouteVillageId,
                    MAX(rv.VisitSequence),
                    1,
                    @UserId,
                    GETDATE()
                FROM RouteSchedules rs
                INNER JOIN RouteVillages rv
                    ON rv.RouteId = rs.RouteId
                WHERE rs.RouteId = @RouteId
                  AND rv.IsActive = 1
                  AND NOT EXISTS
                (
                    SELECT 1
                    FROM RouteVillageSchedules x
                    WHERE x.RouteScheduleId = rs.RouteScheduleId
                      AND x.RouteVillageId   = rv.RouteVillageId
                )
                GROUP BY
                    rs.RouteScheduleId,
                    rv.RouteVillageId";

            using (SqlCommand cmd =
                new SqlCommand(insertQuery, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                cmd.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    userId;

                cmd.ExecuteNonQuery();
            }


            // c. keep the visit order in step with the route
            const string sequenceQuery = @"
                UPDATE rvs
                SET VisitSequence = rv.VisitSequence
                FROM RouteVillageSchedules rvs
                INNER JOIN RouteVillages rv
                    ON rv.RouteVillageId = rvs.RouteVillageId
                INNER JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = rvs.RouteScheduleId
                WHERE rs.RouteId = @RouteId
                  AND rvs.VisitSequence <> rv.VisitSequence";

            using (SqlCommand cmd =
                new SqlCommand(sequenceQuery, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                cmd.ExecuteNonQuery();
            }
        }


        // =============================================================
        // SINGLE ROUTE-DAY
        // =============================================================

        private static void UpdateRouteDay(
            SqlConnection con,
            SqlTransaction transaction,
            int scheduleId,
            object vehicleId,
            object salesmanId,
            object driverId,
            bool isActive,
            int userId)
        {
            const string query = @"
                UPDATE RouteSchedules
                SET VehicleId   = @VehicleId,
                    SalesmanId  = @SalesmanId,
                    DriverId    = @DriverId,
                    IsActive    = @IsActive,
                    UpdatedBy   = @UserId,
                    UpdatedAt   = GETDATE()
                WHERE RouteScheduleId = @RouteScheduleId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value =
                    scheduleId;

                AddAssignmentParameters(
                    cmd, vehicleId, salesmanId, driverId);

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value =
                    isActive;

                cmd.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    userId;

                cmd.ExecuteNonQuery();
            }
        }


        private static int InsertRouteDay(
            SqlConnection con,
            SqlTransaction transaction,
            int routeId,
            string day,
            object vehicleId,
            object salesmanId,
            object driverId,
            bool isActive,
            int userId)
        {
            const string query = @"
                INSERT INTO RouteSchedules
                (
                    RouteId,
                    DayOfWeek,
                    VehicleId,
                    SalesmanId,
                    DriverId,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @RouteId,
                    @DayOfWeek,
                    @VehicleId,
                    @SalesmanId,
                    @DriverId,
                    @IsActive,
                    @UserId,
                    GETDATE()
                );

                SELECT CONVERT(INT, SCOPE_IDENTITY());";

            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                cmd.Parameters.Add(
                    "@DayOfWeek",
                    SqlDbType.NVarChar,
                    40).Value =
                    day;

                AddAssignmentParameters(
                    cmd, vehicleId, salesmanId, driverId);

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value =
                    isActive;

                cmd.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    userId;

                object result = cmd.ExecuteScalar();

                return result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
        }


        private static void DeactivateRouteDay(
            SqlConnection con,
            SqlTransaction transaction,
            int scheduleId,
            int userId)
        {
            const string query = @"
                UPDATE RouteSchedules
                SET IsActive  = 0,
                    UpdatedBy = @UserId,
                    UpdatedAt = GETDATE()
                WHERE RouteScheduleId = @RouteScheduleId
                  AND IsActive = 1";

            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value =
                    scheduleId;

                cmd.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    userId;

                cmd.ExecuteNonQuery();
            }
        }


        // A route may have no vehicle / salesman / driver yet, so
        // these are always nullable.
        private static void AddAssignmentParameters(
            SqlCommand cmd,
            object vehicleId,
            object salesmanId,
            object driverId)
        {
            cmd.Parameters.Add(
                "@VehicleId",
                SqlDbType.Int).Value =
                vehicleId ?? DBNull.Value;

            cmd.Parameters.Add(
                "@SalesmanId",
                SqlDbType.Int).Value =
                salesmanId ?? DBNull.Value;

            cmd.Parameters.Add(
                "@DriverId",
                SqlDbType.Int).Value =
                driverId ?? DBNull.Value;
        }
    }
}
