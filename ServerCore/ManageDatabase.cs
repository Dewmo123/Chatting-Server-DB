using MySql.Data.MySqlClient;
using System;

namespace ServerCore
{
    public class ManageDatabase
    {
        MySqlConnection _mysql;
        MySqlDataReader _table;
        string _tableName;
        public void ConnectDB(string connectionAddress, string tableName)
        {
            _mysql = new MySqlConnection(connectionAddress);
            _tableName = tableName;
        }
        public bool SearchDuplication(string ip)
        {
            Console.WriteLine("Check Duplication");
            _mysql.Open();
            MySqlCommand command = new MySqlCommand($"SELECT ip FROM {_tableName} WHERE ip = '{ip}'", _mysql);
            _table = command.ExecuteReader();
            bool successRead = _table.Read();
            _mysql.Close();
            return successRead;
        }
        public void InsertClientInfo(string ip, string name)
        {
            _mysql.Open();
            if (_table.IsClosed == false) _table.Close();
            MySqlCommand command = new MySqlCommand($"INSERT INTO {_tableName} (ip,clientName) VALUES ('{ip}','{name}')", _mysql);
            Console.WriteLine("AddClient");
            _table = command.ExecuteReader();
            if (_table.RecordsAffected != 1) Console.WriteLine("Error");
            _mysql.Close();
        }
        public string GetClientName(string ip)
        {
            _mysql.Open();
            if (_table.IsClosed == false) _table.Close();
            MySqlCommand command = new MySqlCommand($"SELECT clientName FROM {_tableName} WHERE ip = '{ip}'", _mysql);
            Console.WriteLine("GetName");
            _table = command.ExecuteReader();
            string name = null;
            while (_table.Read())
                name = _table["clientName"].ToString();
            _mysql.Close();
            return name;
        }
    }
}
