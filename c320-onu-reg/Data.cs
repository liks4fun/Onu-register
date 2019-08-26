using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace c320_onu_reg
{
    //Тут у нас устроенно хранилище
    class Data
    {
        //Список с коммутаторами
        private List<Commutator> Commutators { get; set; }

        public Data()
        {
            Commutators = new List<Commutator>();
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<Commutator>));
            using (FileStream fs = new FileStream("data.json", FileMode.OpenOrCreate))
            {
                fs.Position = 0;
                FileInfo file = new FileInfo("data.json");
                if (file.Length != 0)
                    Commutators = (List<Commutator>)jsonFormatter.ReadObject(fs);
            }
        }
        //получаем список всех коммутаторов
        public List<Commutator> GetCommutators()
        {
            return Commutators;
        }

        //Добавляем новый коммутатор
        public void AddCommutator()
        {
            Console.WriteLine("Now we add new device, please fill required fields");
            Console.Write("Device name: ");
            string name = Console.ReadLine();
            Console.Write("\nLogin: ");
            string login = Console.ReadLine();
            Console.Write("\nPassword: ");
            string password = Console.ReadLine();
            Console.Write("\nIP: ");
            string ip = Console.ReadLine();
            Console.Write("\nLine profile name: ");
            string lineProfileName = Console.ReadLine();
            Console.Write("\nRemote profile name: ");
            string remoteProfileName = Console.ReadLine();
            Console.Write("\nManagement vlan id: ");
            int mngvid = Int32.Parse(Console.ReadLine());
            Console.WriteLine("\nPick 1 or 2 for your firmware ver.: ");
            int firmwareVer = Int32.Parse(Console.ReadKey(false).KeyChar.ToString());
            Commutator commutator = new Commutator(name,ip,login,password,lineProfileName,remoteProfileName, firmwareVer, mngvid);

            //добавляем в список
            Commutators.Add(commutator);
            //Сохраняем в файл
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<Commutator>));
            using (FileStream fs = new FileStream("data.json", FileMode.OpenOrCreate))
            {
                jsonFormatter.WriteObject(fs, Commutators);
            }
        }
    }
}
