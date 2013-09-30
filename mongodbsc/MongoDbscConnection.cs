using dbsc.Core;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace dbsc.Mongo
{
    class MongoDbscConnection
    {
        private MongoServer m_server;
        private MongoDatabase m_database;
        private DbConnectionInfo m_connectionInfo;
        
        public MongoDbscConnection(DbConnectionInfo connectionInfo)
        {
            m_connectionInfo = connectionInfo;

            MongoUrl url = GetMongoUrl(m_connectionInfo);

            MongoClient client = new MongoClient(url);
            m_server = client.GetServer();
            m_server.Connect();
            m_database = m_server.GetDatabase(connectionInfo.Database);
        }

        private static MongoUrl GetMongoUrl(DbConnectionInfo connectionInfo)
        {
            MongoUrlBuilder urlBuilder = new MongoUrlBuilder();
            urlBuilder.ConnectTimeout = TimeSpan.FromSeconds(connectionInfo.ConnectTimeoutInSeconds);
            urlBuilder.DatabaseName = connectionInfo.Database;
            urlBuilder.Password = connectionInfo.Password;
            urlBuilder.Username = connectionInfo.Username;

            if (connectionInfo.Port == null)
            {
                urlBuilder.Server = new MongoServerAddress(connectionInfo.Server);
            }
            else
            {
                urlBuilder.Server = new MongoServerAddress(connectionInfo.Server, connectionInfo.Port.Value);
            }

            urlBuilder.SocketTimeout = TimeSpan.FromSeconds(connectionInfo.CommandTimeoutInSeconds);

            return urlBuilder.ToMongoUrl();
        }

        public bool ContainsCollection(string collectionName)
        {
            return m_database.CollectionExists(collectionName);
        }

        public void CreateCollection(string collectionName)
        {
            // Don't use a capped collection because you cannot do updates on documents in a capped collection
            // that would cause the document to grow in size.
            m_database.CreateCollection(collectionName);
        }

        public void Upsert<T>(T document, string collectionName)
        {
            MongoCollection<T> collection = m_database.GetCollection<T>(collectionName);
            collection.Save(document, new MongoInsertOptions() { WriteConcern = new WriteConcern() { WTimeout = TimeSpan.FromSeconds(m_connectionInfo.CommandTimeoutInSeconds) } });
        }

        public void UpdateAll<T>(string collectionName, IEnumerable<Tuple<Expression<Func<T, object>>, object>> updates)
        {
            QueryDocument query = new QueryDocument();
            
            UpdateBuilder<T> updateBuilder = new UpdateBuilder<T>();
            foreach(Tuple<Expression<Func<T, object>>, object> updateLambdaAndObject in updates)
            {
                updateBuilder.Set(updateLambdaAndObject.Item1, updateLambdaAndObject.Item2);
            }

            MongoDB.Driver.MongoUpdateOptions updateOptions = new MongoDB.Driver.MongoUpdateOptions()
            {
                Flags = UpdateFlags.Multi,
                WriteConcern = new WriteConcern() { WTimeout = TimeSpan.FromSeconds(m_connectionInfo.CommandTimeoutInSeconds) }
            };

            MongoCollection collection = m_database.GetCollection(collectionName);
            collection.Update(query, updateBuilder, updateOptions);
        }

        public T GetSingleDocument<T>(string collectionName)
        {
            MongoCollection<T> collection = m_database.GetCollection<T>(collectionName);
            return collection.FindOne();
        }

        public ICollection<string> GetCollectionNames()
        {
            return m_database.GetCollectionNames().ToList();
        }
    }
}

/*
 Copyright 2013 Greg Najda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/