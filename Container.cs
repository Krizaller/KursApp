using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WinFormsApp2
{
    class Container {
        public string container_id { get; set; }
        public int container_status { get; set; }
        public string container_location { get; set; }
        public List<Sensor> Sensors;


        public Sensor this[int index] { 
            get { return Sensors[index]; } 
            set { Sensors[index] = value; } 
        }

        public Container(){
            container_id = "0000";
            container_status = 0;
            container_location = "";
            Sensors = new List<Sensor>();
        }

        public Container(string Container_id, int Container_status, string Container_location,List<Sensor> Sensors) {
            container_id = Container_id;
            container_status = Container_status;
            container_location = Container_location;
            this.Sensors = Sensors;
        }

        public void LoadSensor(List<Sensor> Sensors){
            this.Sensors = Sensors;
        }


        // добавление датчика
        public int AddSensor(Sensor Sensor){
            Sensors.Add(Sensor);
            return 0;
        }

        // удаление датчика
        public void RemoveSensor(int index){
            Sensors.RemoveAt(index);
        }

        public int ColSensor() {
            return Sensors.Count;
        }

        public string lastid(){
            if (Sensors.Count == 0) return "00";
            else return Sensors.Last().sensor_id;
        }
    }
}
