﻿using FSO.Server.Database;
using FSO.Server.Servers.Api;
using FSO.Server.Servers.Api.JsonWebToken;
using FSO.Server.Servers.City;
using FSO.Server.Servers.Lot;
using FSO.Server.Servers.Tasks;
using Ninject.Activation;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.IO;

namespace FSO.Server
{
    public class ServerConfiguration
    {
        public string GameLocation;
        public string SimNFS;

        public DatabaseConfiguration Database;
        public ServerConfigurationservices Services;


        /// <summary>
        /// Secret string used as a key for signing JWT tokens for the admin system
        /// </summary>
        public string Secret;
    }


    public class ServerConfigurationservices
    {
        public ApiServerConfiguration Api;
        public ApiServerConfiguration UserApi;
        public TaskServerConfiguration Tasks;
        public List<CityServerConfiguration> Cities;
        public List<LotServerConfiguration> Lots;
    }

    

    public class ServerConfigurationModule : NinjectModule
    {
        ServerConfiguration GetConfiguration(IContext context)
        {
            //TODO: Allow config path to be overriden in a switch
            var configPath = "config.json";
            if (!File.Exists(configPath))
            {
                throw new Exception("Configuration file, config.json, missing");
            }

            var data = File.ReadAllText(configPath);

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfiguration>(data);
            }catch(Exception ex)
            {
                throw new Exception("Could not deserialize config.json", ex);
            }
        }

        class DatabaseConfigurationProvider : IProvider<DatabaseConfiguration>
        {
            ServerConfiguration Config;

            public DatabaseConfigurationProvider(ServerConfiguration config)
            {
                Config = config;    
            }


            public Type Type
            {
                get
                {
                    return typeof(DatabaseConfiguration);
                }
            }

            public object Create(IContext context)
            {
                return Config.Database;
            }
        }


        class JWTConfigurationProvider : IProvider<JWTConfiguration>
        {
            ServerConfiguration Config;

            public JWTConfigurationProvider(ServerConfiguration config)
            {
                Config = config;
            }


            public Type Type
            {
                get
                {
                    return typeof(JWTConfiguration);
                }
            }

            public object Create(IContext context)
            {
                return new JWTConfiguration() {
                    Key = System.Text.UTF8Encoding.UTF8.GetBytes(Config.Secret)
                };
            }
        }

        public override void Load()
        {
            Bind<ServerConfiguration>().ToMethod(new Func<IContext, ServerConfiguration>(GetConfiguration)).InSingletonScope();
            Bind<DatabaseConfiguration>().ToProvider<DatabaseConfigurationProvider>().InSingletonScope();
            Bind<JWTConfiguration>().ToProvider<JWTConfigurationProvider>().InSingletonScope();
        }
    }
}