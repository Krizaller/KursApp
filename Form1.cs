﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinFormsApp2;
using _Excel = Microsoft.Office.Interop.Excel;
using Conter = WinFormsApp2.Container;

namespace KursApp
{
    public partial class Form1 : Form{

        struct SensorSettingsDB{
            public string sensor_name;
            public double sensor_value;
            public double[] sensor_minmax;

            public SensorSettingsDB(string sensor_name, double sensor_value, double[] sensor_minmax) { 
                this.sensor_minmax = sensor_minmax;
                this.sensor_name = sensor_name;
                this.sensor_value = sensor_value;
            }
        }

        struct sensorvalue{
            public string sensor_name;
            public double sensor_current_value;
        }

        struct CommandData{
            public string Command { get; set; }
            public double Value { get; set; }
        }

        DatabaseHelper db = new DatabaseHelper("localhost", "dbcontainermonitoring", "root", "");
       
        List<Conter> Containers;

        string[] TypeEvent = { "Информация", "Изменение данных", "Получение данных","Запись данных" };
        string CommmandSendContainer = "0000";
        int OperatorIDCurrent = 0;

        bool RecordAction = false;
        
        public Form1(){
            InitializeComponent();
            SwitchProgramm(false);
        }

        //////// Настройка оператора ////////

        private void button14_Click(object sender, EventArgs e)
        {
            if (ValidNameOperator())
            {
                int operatorid = CheckOperator();
                if (operatorid != -1)
                {
                    info("Доступ Разрешен");
                    SwitchProgramm(true);
                    OperatorIDCurrent = operatorid;
                    RecordEvent(0, $"Пользватель {textBox2.Text} {textBox1.Text} вошел в систему");
                }
                else
                {
                    Er("Нет такого человека");
                }

            }

        }

        private void button15_Click(object sender, EventArgs e)
        {
            SwitchProgramm(false);
            RecordEvent(0, $"Пользватель {textBox2.Text} {textBox1.Text} Вышел из системы");
        }

        private void SwitchProgramm(bool Enabled)
        {
            // Enabled = true;
            Control[] containers = { Containers_Monitoring, Containers_Settings, Containers_Report };
            foreach (var container in containers)
            {
                foreach (Control control in container.Controls)
                {
                    control.Enabled = Enabled;
                }
            }
            button15.Enabled = Enabled;
            button14.Enabled = !Enabled;
            textBox1.Enabled = !Enabled;
            textBox2.Enabled = !Enabled;
            textBox3.Enabled = !Enabled;
        }

        private bool ValidNameOperator()
        {
            System.Windows.Forms.TextBox[] textboxs = { textBox1, textBox2, textBox3 };
            foreach (var textbox in textboxs)
            {
                if (textbox.Text.Length <= 1)
                {
                    Er("Не может быть меньше 1 буквы");
                    return false;
                }
            }
            return true;
        }

        private int CheckOperator()
        {
            string name = $"{textBox1.Text}-{textBox2.Text}-{textBox3.Text}";
            int OperatorId = db.GetOperator(name.ToLower());
            return OperatorId;
        }

        //////// Настройка контейнера ////////


        //нахождение контейнеров
        private void button1_Click(object sender, EventArgs e){

            Loadinginformation();
            //string JsonPathSave = "E:/EPorject/EInstitut/KursApp/Informatin/";
            //SaveInContainerInformation(JsonPathSave, "MyJson.json");
        }

        //Загрузка настроек датчика
        private void button7_Click(object sender, EventArgs e)
        {

            List<string> list = db.GetSensorSettingsNames();
            LoadPresetSettings lps = new LoadPresetSettings(list);
            lps.ShowDialog();
            string name = db.GetSensorsFromSensorSettings(lps.SettingsNameChoised);
            List<SensorSettingsDB> SensorSettings;
            try
            {

                SensorSettings = JsonConvert.DeserializeObject<List<SensorSettingsDB>>(name,
                    new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    }
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error deserializing JSON to Container objects: " + ex.Message);
            }
            Conter container = Containers[ContainerIndex(Label_NContainer.Text)];
            foreach (var sensor in container.Sensors)
            {
                var matchingSetting = SensorSettings.FirstOrDefault(s => s.sensor_name == sensor.sensor_name);
                if (matchingSetting.sensor_name != null)
                {
                    sensor.sensor_value = matchingSetting.sensor_value;
                    sensor.sensor_minmax = matchingSetting.sensor_minmax;
                }
            }
            UpdateTables();

            RecordEvent(2, $"Загрузка настроек '{lps.SettingsNameChoised}' из таблицы");
        }

        //Сохранение настроек датчика
        private void button6_Click(object sender, EventArgs e)
        {
            string SettingsName = ShowInputDialog("Введите Названия настройки");
            if (SettingsName == "-1")
            {
                Er("Не было введено имя настройки");
                return;
            }

            Conter container = Containers[ContainerIndex(Label_NContainer.Text)];
            List<SensorSettingsDB> SensorSettings = new List<SensorSettingsDB>();
            foreach (var sensor in container.Sensors)
            {
                SensorSettings.Add(new SensorSettingsDB(sensor.sensor_name, sensor.sensor_value, sensor.sensor_minmax));
            }

            string json = JsonConvert.SerializeObject(SensorSettings);
            db.InsertSensorSetting(SettingsName, json);
            RecordEvent(1, $"Сохранение настроек '{SettingsName}' в таблицу");
        }


        //изменение контейнера
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e){
            if (e.ColumnIndex == dataGridView2.Columns["container_change"].Index && e.RowIndex >= 0){
                int row = e.RowIndex;
                ContainerChangeInformation(dataGridView2.Rows[row].Cells[0].Value.ToString());        
            }

        }

        //изменение датчиков
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.ColumnIndex == dataGridView1.Columns["Sensor_Change"].Index && e.RowIndex >= 0)
            {

                int row = e.RowIndex;
                string containerid = Label_NContainer.Text;

                SensorChangeInformation(containerid, dataGridView1.Rows[row].Cells[0].Value.ToString());
            }
        }

        //изменение информации о контейнере
        private void ContainerChangeInformation(string ContainerId) {      
            var container = Containers.FirstOrDefault(c => c.container_id == ContainerId);
            if (container != null){
                ChangeContainerForm changeContainerForm = new ChangeContainerForm();
                using (changeContainerForm){
                    if (changeContainerForm.ShowDialog() == DialogResult.OK){
                        container.container_id = changeContainerForm.IDContainer;
                        for (int i = 0; i < container.Sensors.Count; i++)
                        {
                            container.Sensors[i].sensor_id = $"{container.container_id}{i:D2}";
                        }
                        UpdateTables();
                    }
                }

            }

        }
        
        //изменение информации о датчике
        private void SensorChangeInformation(string ContainerId, string sensorId) {
            var container = Containers.FirstOrDefault(c => c.container_id == ContainerId);
            var sensor = container.Sensors.FirstOrDefault(s => s.sensor_id == sensorId);
            if (container != null && sensor != null)
            {
                ChangeSensorForm csf = new ChangeSensorForm(sensor.sensor_id, sensor.sensor_name, sensor.sensor_type_value, sensor.sensor_value, sensor.sensor_minmax);
                using (csf)
                {
                    if (csf.ShowDialog() == DialogResult.OK)
                    {
                        sensor.sensor_id = csf.sensor_id;
                        sensor.sensor_name = csf.sensor_name;
                        sensor.sensor_type_value = csf.sensor_type_value;
                        sensor.sensor_value = csf.sensor_value;
                        sensor.sensor_minmax = csf.sensor_minmax;
                        UpdateTables();
                    }
                }

            }

        }

        //отображение во второй талице
        private void dataGridView2_SelectionChanged(object sender, EventArgs e){
            UpdateTable2();
        }
   
        //Обновление таблицы 1 (контейнеры)
        private void UpdateTable1() {
            int rowIndex;
            dataGridView2.Rows.Clear();
            for (int i=0;i<Containers.Count;i++) {
                rowIndex = dataGridView2.Rows.Add();
                dataGridView2.Rows[rowIndex].Cells[0].Value = Containers[i].container_id;
                dataGridView2.Rows[rowIndex].Cells[1].Value = Containers[i].container_status;
                dataGridView2.Rows[rowIndex].Cells[2].Value = Containers[i].ColSensor() ;
            }
        }

        //Обновление таблицы 2 (датчики)
        private void UpdateTable2(){
            if (dataGridView2.CurrentRow != null){
                int selectedRow = dataGridView2.CurrentRow.Index;

                Label_NContainer.Text = Containers[selectedRow].container_id;

                dataGridView1.Rows.Clear();
                int rowIndex;
                for (int i = 0; i < Containers[selectedRow].Sensors.Count; i++){
                    rowIndex = dataGridView1.Rows.Add();
                    dataGridView1.Rows[rowIndex].Cells[0].Value = Containers[selectedRow].Sensors[i].sensor_id;
                    dataGridView1.Rows[rowIndex].Cells[1].Value = Containers[selectedRow].Sensors[i].sensor_name;
                    dataGridView1.Rows[rowIndex].Cells[2].Value = Containers[selectedRow].Sensors[i].sensor_type_value;
                    dataGridView1.Rows[rowIndex].Cells[3].Value = Containers[selectedRow].Sensors[i].sensor_value;
                    dataGridView1.Rows[rowIndex].Cells[4].Value = Containers[selectedRow].Sensors[i].sensor_minmax[0].ToString() + ";" + Containers[selectedRow].Sensors[i].sensor_minmax[1].ToString();
                }
            }
        }

        //загрузка данных из файла
        public void LoadContainerInformation(string path){
            try{
                string json = File.ReadAllText(path);
                Containers = JsonConvert.DeserializeObject<List<Conter>>(json, 
                    new JsonSerializerSettings{
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    }
                );
                RecordEvent(2, $"Получены данные, Конейров {Containers.Count}");
            }
            catch (Exception ex){
                throw new Exception("Error deserializing JSON to Container objects: " + ex.Message);
            }
            
        }


        //////// Мониторинг ////////

        //найти контейнеры
        private void button3_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        //начать мониторинг
        private void button5_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
            MonitorStatus(true);
            timer1.Start();
            RecordEvent(0, $"Был начат мониторинг контейнеров");
        }

        //остановить мониторинг
        private void button13_Click(object sender, EventArgs e)
        {
            MonitorStatus(false);
            timer1.Stop();
            RecordEvent(0, $"Был остановлен мониторинг контейнеров");
        }

        //кнопка опроса контейнеров
        private void button2_Click(object sender, EventArgs e){
            UpdateTable3();
            RecordEvent(0, $"Оператор вручную опросил контейнеры");
        }

        //получение значений датчиков (мониторинг)
        private string GetSensorValue(string containerID) {
            string path = $"E:/EPorject/EInstitut/KursApp/Informatin/SensorValue{containerID}.json";
            return File.ReadAllText(path);
        }

        //обновление таблицы мониторинга
        private void UpdateTable3() {
           
            for (int c = 0; c < Containers.Count; c++) { 
         
                string json = GetSensorValue(Containers[c].container_id);
                ContainerRecordsValues(Containers[c].container_id, json);

                var sensors = JsonConvert.DeserializeObject<List<sensorvalue>>(json,
                    new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    }
                );
                

                foreach (var sensor in sensors){
                    for (int i = 0; i < dataGridView3.Columns.Count; i++){
                        if (dataGridView3.Columns[i].Name == sensor.sensor_name){
                            var sensorname = Containers[c].Sensors.FirstOrDefault(s => s.sensor_name == sensor.sensor_name);
                            if (sensorname == null) { dataGridView3.Rows[c].Cells[i].Style.BackColor = Color.Black; continue; }
                            dataGridView3.Rows[c].Cells[i].Value = sensor.sensor_current_value;
                            if (sensorname.sensor_minmax[0] > sensor.sensor_current_value || sensorname.sensor_minmax[1] < sensor.sensor_current_value)
                            {
                                dataGridView3.Rows[c].Cells[i].Style.BackColor = Color.Red;
                            }
                            else {
                                dataGridView3.Rows[c].Cells[i].Style.BackColor = Color.LightGreen;
                            }
                        }
                    }
                }
            }

        }

        //загрузка датчиков для таблицы мониторинга
        private void LoadTableMonitoring(){
            dataGridView3.Rows.Clear();
            dataGridView3.Columns.Clear();
            HashSet<string> uniqueSensorNames = new HashSet<string>();
            HashSet<string> uniqueSensorNamesValue = new HashSet<string>();

            // Перебор контейнеров и сенсоров
            foreach (var container in Containers){
                foreach (var sensor in container.Sensors){
                    uniqueSensorNames.Add(sensor.sensor_name);
                    uniqueSensorNamesValue.Add($"{sensor.sensor_name} {sensor.sensor_type_value}");
                }
            }

            string[] uniqueSensorsArray = new string[uniqueSensorNames.Count];
            uniqueSensorNames.CopyTo(uniqueSensorsArray);
            string[] uniqueSensorArrayValue = new string[uniqueSensorNamesValue.Count];
            uniqueSensorNamesValue.CopyTo(uniqueSensorArrayValue);

            dataGridView3.Columns.Add("MonitorContainer", "Контейнер");
            for (int j = 0; j < uniqueSensorsArray.Length; j++){
                dataGridView3.Columns.Add(uniqueSensorsArray[j], uniqueSensorArrayValue[j]);
            }

            foreach (var container in Containers){
                int row = dataGridView3.Rows.Add();
                int cour = dataGridView3.ColumnCount;
                dataGridView3.Rows[row].Cells[0].Value = container.container_id;
            }

        }

        //обновления значения для котроля за конйтенером
        private void UpdateContainerControl() {
            int selectedRow = dataGridView3.CurrentRow.Index;
            label11.Text = Containers[selectedRow].container_id;
            CommmandSendContainer = Containers[selectedRow].container_id;
        }
     
        //переключения значений надписи
        private void MonitorStatus(bool status) {
            if (status)
            {
                label16.Text = "Включенно";
                label16.ForeColor = Color.Green;
            }
            else {
                label16.Text = "Выключенно";
                label16.ForeColor = Color.Red;
            }
        }
 
        //выполнения таймера
        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateTable3();
        }

        //Управление конейнером
        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            UpdateContainerControl();
        }

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommand("OpenHum");
        }

        private void button9_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommand("CloseHum");
        }

        private void button10_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommand("OpenCO2");
        }

        private void button10_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommand("CloseCO2");
        }

        private void button11_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommand("OpenO2");
        }

        private void button11_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommand("CloseO2");
        }

        private void button12_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommand("OpenAZOT");
        }

        private void button12_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommand("CloseAZOT");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SendCommand($"TemperatureChange", (int)numericUpDown1.Value);
        }

        private void SendCommand(string command){
            SendCommand(command, 0);   
        }

        private void SendCommand(string command, int value){
            string json = JsonConvert.SerializeObject(new CommandData
            {
                Command = command,
                Value = value
            });
            string data = $"{CommmandSendContainer}|{json}";
            RecordEvent(1, $"Оператор Изменил среду контейнера, комманда:'{data}'");
            Console.WriteLine(data);
            ChangeJSONSensorValue(data);
        }

        //имитация монитооринга
        private void ChangeJSONSensorValue(string data) {
            string[] dataparse = data.Split('|');

            string path = $"E:/EPorject/EInstitut/KursApp/Informatin/SensorValue{dataparse[0]}.json";

            var Coomand = JsonConvert.DeserializeObject<CommandData>(dataparse[1],
                    new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    }
            );


            string json = File.ReadAllText(path);
            var sensors = JsonConvert.DeserializeObject<List<sensorvalue>>(json);

            switch (Coomand.Command)
            {
                case "TemperatureChange":
                    {
                        var index1 = sensors.FindIndex(s => s.sensor_name == "Температура");
                        var sensor1 = sensors[index1];
                        sensor1.sensor_current_value = Coomand.Value;
                        sensors[index1] = sensor1;
                        break;
                    }
                case "OpenO2":
                    {
                        var index = sensors.FindIndex(s => s.sensor_name == "Давление");
                        var sensor = sensors[index];
                        sensor.sensor_current_value = sensor.sensor_current_value + 0.01;
                        sensors[index] = sensor;
                        break;
                    }
                default: { break; }
            }

            json = JsonConvert.SerializeObject(sensors, Formatting.Indented);
            File.WriteAllText(path, json);

        }

        //настрока таймера опросв
        private void numericUpDown2_ValueChanged(object sender, EventArgs e){
            timer1.Interval = (int)numericUpDown2.Value * 1000;
            RecordEvent(1, $"Изменено Время опроса до {timer1.Interval.ToString()} секунд");
        }


        //////// Отчетность ////////

        private void button16_Click(object sender, EventArgs e)
        {
            Report_Container(label21.Text, label21.Text);   
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Report_Event(label23.Text, label23.Text);
        }
        
        private void Report_Event(string Event_Type, string FileName)
        {
            var events = db.GetEvents(Event_Type);

            _Excel.Application exelapp = new _Excel.Application();
            if (exelapp == null)
            {
                Er("Exel не установлен");
                return;
            }
            exelapp.Workbooks.Add();
            _Excel._Worksheet worksheet = (_Excel._Worksheet)exelapp.ActiveSheet;
            _Excel.Range range = worksheet.Cells[1, 1];

            worksheet.Cells[1, 1] = Event_Type;
            worksheet.Cells[2, 1] = "Id Контейнера";
            worksheet.Cells[2, 2] = "Оператор";
            worksheet.Cells[2, 3] = "Описание события";
            worksheet.Cells[2, 4] = "Время события";

            int row = 3;
            foreach (var e in events)
            {
                worksheet.Cells[row, 1] = e.Id_Container;
                worksheet.Cells[row, 2] = db.GetOperatorName(e.Id_Operator);
                worksheet.Cells[row, 3] = e.Description_Report;
                worksheet.Cells[row, 4] = e.Time_Report;
                row++;
            }
            range = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[row, 5]];

            range.Font.Name = "Times New Roman";
            range.Font.Size = 14;
            range.HorizontalAlignment = _Excel.XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = _Excel.XlVAlign.xlVAlignCenter;
            worksheet.Columns.AutoFit();
            worksheet.Rows.AutoFit();


            var path = $@"E:\EPorject\EInstitut\KursApp\Informatin\{FileName}.xlsx";
            worksheet.SaveAs(path);
            info("Файл сформирован");
            exelapp.Quit();
            RecordEvent(3, $"Запись отчета события {Event_Type}");

        }

        private void Report_Container(string ContainerID, string FileName)
        {
            _Excel.Application exelapp = new _Excel.Application();
            if (exelapp == null)
            {
                Er("Exel не установлен");
                return;
            }
            exelapp.Workbooks.Add();
            _Excel._Worksheet worksheet = (_Excel._Worksheet)exelapp.ActiveSheet;
            _Excel.Range range = worksheet.Cells[1, 1];

            Conter coner = Containers[ContainerIndex(ContainerID)];

            // Заполнение информации о контейнере
            worksheet.Cells[1, 1] = "Контейнер";
            worksheet.Cells[2, 1] = "Локация";
            worksheet.Cells[3, 1] = "Статус";
            worksheet.Cells[1, 2] = coner.container_id;
            worksheet.Cells[2, 2] = coner.container_location;
            worksheet.Cells[3, 2] = coner.container_status;

            worksheet.Cells[4, 2] = "Датчик Id";
            worksheet.Cells[5, 2] = "Имя";
            worksheet.Cells[6, 2] = "Тип";
            worksheet.Cells[7, 2] = "Значение";
            worksheet.Cells[8, 2] = "Пределы";

            // Заполнение данных по датчикам
            int row = 3;
            foreach (var sensor in coner.Sensors)
            {

                worksheet.Cells[4, row] = sensor.sensor_id;
                worksheet.Cells[5, row] = sensor.sensor_name;
                worksheet.Cells[6, row] = sensor.sensor_type_value;
                worksheet.Cells[7, row] = sensor.sensor_value;
                worksheet.Cells[8, row] = $"{sensor.sensor_minmax[0]}, {sensor.sensor_minmax[1]}";

                row += 1;
            }
            range = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[8, row]];

            range.Font.Name = "Times New Roman";
            range.Font.Size = 14;
            range.HorizontalAlignment = _Excel.XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = _Excel.XlVAlign.xlVAlignCenter;
            worksheet.Columns.AutoFit();
            worksheet.Rows.AutoFit();


            var path = $@"E:\EPorject\EInstitut\KursApp\Informatin\{FileName}.xlsx";
            worksheet.SaveAs(path);
            info("Файл сформирован");
            exelapp.Quit();
            RecordEvent(3, $"Запись отчета контейнера: {ContainerID}");
        }

        //Обновление таблицы 5 (Отчет Собития)
        private void UpdateTabel5()
        {
            dataGridView5.Rows.Clear();
            for (int i = 0; i < TypeEvent.Length; i++)
            {
                int a = dataGridView5.Rows.Add();
                dataGridView5.Rows[a].Cells[0].Value = TypeEvent[i];
                dataGridView5.Rows[a].Cells[1].Value = db.GetEventsCount(TypeEvent[i]).ToString();
            }
        }

        //Обновление таблицы 4 (Отчет контейнеры)
        private void UpdateTable4()
        {
            int rowIndex;
            dataGridView4.Rows.Clear();
            for (int i = 0; i < Containers.Count; i++)
            {
                rowIndex = dataGridView4.Rows.Add();
                dataGridView4.Rows[rowIndex].Cells[0].Value = Containers[i].container_id;
                dataGridView4.Rows[rowIndex].Cells[1].Value = Containers[i].container_status;
                dataGridView4.Rows[rowIndex].Cells[2].Value = Containers[i].ColSensor();
            }
        }

        private void dataGridView4_SelectionChanged(object sender, EventArgs e)
        {
            label21.Text = Containers[dataGridView4.CurrentRow.Index].container_id;
        }

        private void dataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            label23.Text = TypeEvent[dataGridView5.CurrentRow.Index];
        }


        //////// остальные функции ////////

        //получения индекса через id контейнера
        public int ContainerIndex(string ContainerID)
        {

            if (Containers.Count <= 0) { return -1; }

            for (int i = 0; i < Containers.Count; i++)
            {
                if (Containers[i].container_id == ContainerID) { return i; }
            }

            return -1;
        }

        //форма для ввода имени настройки 
        private string ShowInputDialog(string prompt)
        {
            Form inputForm = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Введите значение",
                StartPosition = FormStartPosition.CenterScreen
            };

            System.Windows.Forms.Label label = new System.Windows.Forms.Label() { Left = 10, Top = 20, Text = prompt, Width = 360 };
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Left = 10, Top = 50, Width = 360 };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "OK", Left = 290, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            inputForm.Controls.Add(label);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(confirmation);
            inputForm.AcceptButton = confirmation;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : "-1";
        }

        private void Er(string Text)
        {
            MessageBox.Show(Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void info(string Text)
        {
            MessageBox.Show(Text, "Инофрмация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateTables()
        {
            LoadTableMonitoring();
            UpdateTable1();
            UpdateTable2();
            UpdateTable4();
            UpdateTabel5();
        }

        //main функция для загрузки всего
        private void Loadinginformation()
        {
            string JsonPathLoad = "E:/EPorject/EInstitut/KursApp/Informatin/ContainerInfo.json";
            LoadContainerInformation(JsonPathLoad);  
            UpdateTables();
            RecordEvent(2, $"Запрос на получение данных c контейнеров");
            RecordContainers();
        }

        /// <summary>
        /// запись в таблицу собития
        /// 0 - "Информация", 1 - "Изменение данных", 2 - "Получение данных" 3- "Запись данных"
        /// </summary>
        /// <param name="TypeReport"></param>
        private void RecordEvent(int TypeReport, string DescriptionReport){
            RecordEvent("0000", TypeEvent[TypeReport], DescriptionReport);
        }

        // запись в таблицу собития
        private void RecordEvent(string ContainerID, string TypeReport, string DescriptionReport)
        {
            if (RecordAction){
                db.InsertEvent(ContainerID, OperatorIDCurrent, TypeReport, DescriptionReport);
            }
        }

        // запись в таблицу данных о полученых контейнеров
        private void RecordContainers(){
            if (RecordAction){
                foreach (var Container in Containers)
                {
                    db.InsertContainer(Container.container_id, Container.container_location, Container.container_status.ToString());
                    foreach (var sensor in Container.Sensors)
                    {
                        db.InsertSensor(sensor, Container.container_id);
                    }
                }
            }
        }
        private void ContainerRecordsValues(string ContainerID, string SensorValueJSON){
            if (RecordAction){
                db.InsertContainerRecords(ContainerID, SensorValueJSON);
            }
        }
    }
}
