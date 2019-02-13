namespace FSO.Server.Database.DA
{
    public class MySqlDAFactory : IDAFactory
    {
        DatabaseConfiguration _config;

        public MySqlDAFactory(DatabaseConfiguration config)
        {
            _config = config;
        }

        public IDA Get => new SqlDA(new MySqlContext(_config.ConnectionString));
    }
}
