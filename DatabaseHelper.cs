using Microsoft.Office.Interop.Excel;
using MySql.Data.MySqlClient;
using Mysqlx.Session;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using WinFormsApp2;

class DatabaseHelper{

    private string connectionString;

    MySqlConnection connection;
    public DatabaseHelper(string server, string database, string username, string password){
        connectionString = $"Server={server};Database={database};Uid={username};Pwd={password};";
        connection = new MySqlConnection(connectionString);
    }

    // Открыть соединение
    private void OpenConnection(){
        if (connection.State == System.Data.ConnectionState.Closed){
            connection.Open();
        }
    }

    // Закрыть соединение
    private void CloseConnection(){
        if (connection.State == System.Data.ConnectionState.Open){
            connection.Close();
        }
    }

    // Функция добавления в таблицу Containers
    public bool InsertContainer(string idContainer, string location, string status){
        try{
            OpenConnection();
            string query = "INSERT INTO Containers (Id_Container, Location, Status) VALUES (@idContainer, @location, @status)";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@idContainer", idContainer);
            command.Parameters.AddWithValue("@location", location);
            command.Parameters.AddWithValue("@status", status);
            return command.ExecuteNonQuery() > 0;
        }
        finally{
            CloseConnection();
        }
    }


    // Функция получения данных из таблицы Containers
    public List<Container> GetContainers(){
        try{
            OpenConnection();
            string query = "SELECT Id_Container, Location, Status FROM Containers";
            var command = new MySqlCommand(query, connection);
            var reader = command.ExecuteReader();
            List<Container> containers = new List<Container>();
            while (reader.Read()) {
                containers.Add(new Container(reader.GetString(0).ToString(),0, reader.GetString(1), new List<Sensor>()));
            }
            return containers;
        }
        finally{
            CloseConnection();
        }
    }

    // Функция добавления в таблицу Sensor_Settings
    public bool InsertSensorSetting(string nameSetting, string SensorsJSON){
        try {
            OpenConnection();
          
            var command1 = new MySqlCommand(@"SELECT Id_Settings FROM Sensor_Settings WHERE Name_Settings = @nameSetting", connection);
            command1.Parameters.AddWithValue("@nameSetting", nameSetting);
            var res = command1.ExecuteScalar();
            string SettingsId = (res != null ? res.ToString() : "-1");

            string query;
            if (SettingsId != "-1") {
                query = @"UPDATE Sensor_Settings SET Sensors = @SensorsJSON WHERE Name_Settings = @nameSetting";
            } else {
                query = @" INSERT INTO Sensor_Settings (Name_Settings, Sensors) VALUES (@nameSetting, @SensorsJSON)";
            }
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@nameSetting", nameSetting);
            command.Parameters.AddWithValue("@SensorsJSON", SensorsJSON);
            return command.ExecuteNonQuery() > 0;
        }
        finally{
            CloseConnection();
        }
    }

    // Получение датчиков по имени настройки из таблицы Sensor_Settings
    public string GetSensorsFromSensorSettings(string SensorSettingsName){
        try{
            OpenConnection();
            string query = "SELECT Sensors FROM Sensor_Settings WHERE Name_Settings = @SensorSettingsName";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@SensorSettingsName", SensorSettingsName);

            var result = command.ExecuteScalar();
            return result != null ? result.ToString() : "-1";
        }
        finally
        {
            CloseConnection();
        }
    }
    // Получение данных из таблицы Sensor_Settings по ID
    public string GetSensorSettingsName(int SensorSettingsID)
    {
        try
        {
            OpenConnection();
            string query = "SELECT Name_Settings FROM Sensor_Settings WHERE Id_Settings = @SensorSettingsID";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@SensorSettingsID", SensorSettingsID);
            var reader = command.ExecuteReader();
            string result = null;
            if (reader.Read())
            {
                result = reader.GetString(0);
            }
            return result;
        }
        finally
        {
            CloseConnection();
        }
    }

    public List<string> GetSensorSettingsNames()
    {
        try
        {
            OpenConnection();
            string query = "SELECT Name_Settings FROM Sensor_Settings";
            var command = new MySqlCommand(query, connection);
            var reader = command.ExecuteReader();
            List<string> settingsNames = new List<string>();
            while (reader.Read()){
                settingsNames.Add(reader["Name_Settings"].ToString());
            }
            return settingsNames;
        }
        finally
        {
            CloseConnection();
        }
    }

    // Получение данных из таблицы Sensor_Settings по имени
    public string GetSensorSettingsID(string SensorSettingsName)
    {
        try
        {
            OpenConnection();
            string query = "SELECT Id_Settings FROM Sensor_Settings WHERE Name_Settings = @SensorSettingsName";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@SensorSettingsName", SensorSettingsName);
            var result = command.ExecuteScalar();
            return result?.ToString();
        }
        finally{
                CloseConnection();
        }
    }

    public bool InsertEvent(string ContainerID, int OperatorID,string TypeReport,string DescriptionReport){
        try {
            OpenConnection();
            string query = "INSERT INTO events (Id_Container, Id_Operator, Type_Report, Description_Report,Time_Report) VALUES (@ContainerID, @OperatorID, @TypeReport, @DescriptionReport, @TimeReport)";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ContainerID", ContainerID);
            command.Parameters.AddWithValue("@OperatorID", OperatorID);
            command.Parameters.AddWithValue("@TypeReport", TypeReport);
            command.Parameters.AddWithValue("@DescriptionReport", DescriptionReport);
            command.Parameters.AddWithValue("@TimeReport", DateTime.Now);
            return command.ExecuteNonQuery() > 0;
        }
        finally
        {
            CloseConnection();
        }
    }

    // Получение данных из таблицы Events по типу собития
    public List<(int Id_Report, string Id_Container, int Id_Operator, string Description_Report, DateTime Time_Report)> GetEvents(string TypeEvent)
    {
        try
        {
            OpenConnection();
            string query = "SELECT Id_Report, Id_Container, Id_Operator, Description_Report, Time_Report FROM events WHERE Type_Report = @TypeEvent";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@TypeEvent", TypeEvent);
            var reader = command.ExecuteReader();

            var Events = new List<(int, string, int, string, DateTime)>();
            while (reader.Read()){

                Events.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3), reader.GetDateTime(4)));
            }
            return Events;
        }
        finally
        {
            CloseConnection();
        }
    }

    public int GetEventsCount(string TypeEvent){
        try{
            OpenConnection();
            string query = "SELECT COUNT(*) FROM events WHERE Type_Report = @TypeEvent";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@TypeEvent", TypeEvent);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        finally
        {
            CloseConnection();
        }
    }

    public bool InsertOperator(string name, string levelAccess) {
        try
        {
            OpenConnection();
            string query = "INSERT INTO Operators (Name, Level_Access) VALUES (@name, @levelAccess)";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@levelAccess", levelAccess);
            return command.ExecuteNonQuery() > 0;
        }
        finally
        {
            CloseConnection();
        }
    }

    public List<(int IdOperator, string Name, string LevelAccess)> GetOperators(){
        try
        {
            OpenConnection();
            string query = "SELECT Id_Operator, Name, Level_Access FROM Operators";
            var command = new MySqlCommand(query, connection);
            var reader = command.ExecuteReader();
            var operators = new List<(int, string, string)>();
            while (reader.Read())
            {
                operators.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
            }
            return operators;
        }
        finally
        {
            CloseConnection();
        }
    }

    public int GetOperator(string NameOperator)
    {
        try
        {
            OpenConnection();
            string query = "SELECT Id_Operator FROM Operators WHERE Name = @NameOperator";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@NameOperator", NameOperator);
            var reader = command.ExecuteReader();
            int result = -1;
            if (reader.Read())
            {
                result =  reader.GetInt32(0);
            }
            return result;
        }
        finally
        {
            CloseConnection();
        }
    }

    public string GetOperatorName(int OperatorID)
    {
        try
        {
            OpenConnection();
            string query = "SELECT Name FROM Operators WHERE Id_Operator = @OperatorID";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@OperatorID", OperatorID);
            var result = command.ExecuteScalar();
            return result != null ? result.ToString() : null;
        }
        finally
        {
            CloseConnection();
        }
    }


    public bool InsertSensor(Sensor sensor, string containerId)
    {
        try
        {
            OpenConnection();
            string query = "INSERT INTO Sensors (Id_Sensor, Id_Container, Sensor_Name, Sensor_Value_Type, Sensor_Value, Sensor_Min, Sensor_Max) " +
                           "VALUES (@sensor_id, @container_id, @sensor_name, @sensor_value_type, @sensor_value, @sensor_min, @sensor_max)";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@sensor_id", sensor.sensor_id);
            command.Parameters.AddWithValue("@container_id", containerId);
            command.Parameters.AddWithValue("@sensor_name", sensor.sensor_name);
            command.Parameters.AddWithValue("@sensor_value_type", sensor.sensor_type_value);
            command.Parameters.AddWithValue("@sensor_value", sensor.sensor_value);
            command.Parameters.AddWithValue("@sensor_min", sensor.sensor_minmax[0]);
            command.Parameters.AddWithValue("@sensor_max", sensor.sensor_minmax[1]);
            return command.ExecuteNonQuery() > 0;
        }
        finally
        {
            CloseConnection();
        }
    }

    public List<Sensor> GetSensors(string containerId)
    {
        try
        {
            OpenConnection();
            string query = "SELECT Id_Sensor, Sensor_Name, Sensor_Value_Type, Sensor_Value, Sensor_Min, Sensor_Max FROM Sensors WHERE Id_Container = @container_id";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@container_id", containerId);
            var reader = command.ExecuteReader();
            var sensors = new List<Sensor>();

            while (reader.Read())
            {
                var sensor = new Sensor
                {
                    sensor_id = reader.GetString(0),
                    sensor_name = reader.GetString(1),
                    sensor_type_value = reader.GetString(2),
                    sensor_value = reader.GetDouble(3),
                    sensor_minmax = new double[] { reader.GetDouble(4), reader.GetDouble(5) }
                };
                sensors.Add(sensor);
            }
            return sensors;
        }
        finally
        {
            CloseConnection();
        }
    }

    
    public bool InsertContainerRecords(string ContainerID, string ValueSensorRecordJSON){
        try{
            OpenConnection();
            string query = "INSERT INTO container_records (Id_Container, Value_Sensors, Time_Value) VALUES (@ContainerID, @ValueSensorRecordJSON, @Time)";
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ContainerID", ContainerID);
            command.Parameters.AddWithValue("@ValueSensorRecordJSON", ValueSensorRecordJSON);
            command.Parameters.AddWithValue("@Time", DateTime.Now);
            return command.ExecuteNonQuery() > 0;
        }
        finally
        {
            CloseConnection();
        }
    }

    public bool InsertTest(string Text) { 
        connection.Open();
        string query = $" INSERT INTO testtable (text) VALUES ('{Text}')";

        var comand = new MySqlCommand(query, connection);

        return comand.ExecuteNonQuery() > 0;
    }
}
