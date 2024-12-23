using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WinFormsApp2{
    class Sensor{
        public string sensor_id { get; set; }
        public string sensor_name { get; set; }
        public string sensor_type_value { get; set; }
        public double sensor_value { get; set; }
        public double[] sensor_minmax { get; set; }

        public Sensor() {
            sensor_id = "0";
            sensor_name = "Температура";
            sensor_type_value = "C°";
            sensor_value = 0;
            sensor_minmax = new double[2] {10, 20};  
        }
        public Sensor(string sensor_id, string sensor_name) {
            this.sensor_id = sensor_id;
            this.sensor_name = sensor_name;
            sensor_type_value = "C°";
            sensor_value = 0;
            sensor_minmax = new double[2] {10, 20};  
        }

        public Sensor(string sensor_id, 
            string sensor_name, 
            string sensor_description, 
            string sensor_type_value,
            double sensor_value, 
            double[] sensor_minmax ) {

            this.sensor_id = sensor_id;
            this.sensor_name = sensor_name;
            this.sensor_type_value = sensor_type_value;
            this.sensor_value = sensor_value;
            this.sensor_minmax = sensor_minmax;

        }

        public string ShowDate() {
            return "Ид датчика:" + sensor_id +
            "\nИмя датчика:" + sensor_name +
            "\nТип измеряемого значения:" + sensor_type_value +
            "\nзанчение:" + sensor_value +
            "\nпределы значения min max:" + sensor_minmax;
        }
    }
}
