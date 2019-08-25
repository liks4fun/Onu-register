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
            Console.WriteLine("Сейчас мы добавим сюда новый коммутатор, заполни необходимую инфу");
            Console.Write("Имя: ");
            string name = Console.ReadLine();
            Console.Write("\nЛогин: ");
            string login = Console.ReadLine();
            Console.Write("\nПароль: ");
            string password = Console.ReadLine();
            Console.Write("\nIP: ");
            string ip = Console.ReadLine();
            Console.Write("\nИмя профиля line: ");
            string lineProfileName = Console.ReadLine();
            Console.Write("\nИмя профиля remote: ");
            string remoteProfileName = Console.ReadLine();
            Console.Write("\nУкажи 1 для версии прошивки 1+ или 2 для версии 2+: ");
            int type = Int32.Parse(Console.ReadKey().KeyChar.ToString());
            Commutator commutator = new Commutator(name,ip,login,password,lineProfileName,remoteProfileName, type);

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
