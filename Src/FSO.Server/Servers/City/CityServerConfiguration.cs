using FSO.Server.Framework.Aries;

namespace FSO.Server.Servers.City
{
    public class CityServerConfiguration : AbstractAriesServerConfig
    {
        public int ID;

        public CityServerMaintenanceConfiguration Maintenance;
    }

    public class CityServerMaintenanceConfiguration
    {
        public string Cron;
        public int Timeout = 3600;
        public int Visits_Retention_Period = 7;
    }
}
