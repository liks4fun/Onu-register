using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace c320_onu_reg
{
    [DataContract]
    class Commutator
    {
        //Вполне очевидные переменные
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Ip { get; set; }
        [DataMember]
        public string Login { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string LineProfileName { get; set; }
        [DataMember]
        public string RemoteProfileName { get; set; }

        [DataMember]
        private int firmwareVer;

        //Вот тут 1 для v1 прошивки и 2 для v2
        public int FirmwareVer
        {
            get { return firmwareVer; }
            set
            {
                if (value != 1 && value != 2)
                    throw new Exception("Only 1 or 2 allowed.");
                else
                    firmwareVer = value;
            }
        }
        //Переменные для регистрации ону
        public string Sn { get; set; }
        public int OltNum { get; set; }
        public int ShelfNum { get; set; }
        public int OnuIfNum { get; set; }
        public int OnuId { get; set; }
        //Переменная для подключения используя телнет
        public TelnetConnection telnet;

        //Конструктор с минимумом необходимой инфы
        public Commutator(string Name, string Ip, string Login, string Password, string LineProfileName, string RemoteProfileName, int FirmwareVer)
        {
            this.Name = Name;
            this.Ip = Ip;
            this.Login = Login;
            this.Password = Password;
            this.LineProfileName = LineProfileName;
            this.RemoteProfileName = RemoteProfileName;
            this.FirmwareVer = FirmwareVer;
        }

        //Подключаемся и авторизуемся
        public void Auth()
        {
            Console.WriteLine("Trying to connect...");
            telnet = new TelnetConnection(Ip, 23);
            Console.WriteLine("Connected, use login/pass");
            telnet.Login(Login, Password, 300);
            Console.WriteLine("Done, here we are.");
            Console.WriteLine(telnet.Read());
        }

        //Получаем незареганные ону
        public void GetUncfgOnu()
        {
            telnet.WriteLine("show gpon onu uncfg");
            string output = telnet.Read();
            if (output.Contains("No related information"))
            {
                throw new Exception("There is no uncofigured ONU here.");
            }
            else
            {
                //Парсим вывод
                string[] parsedOutput = output.Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
                //Выяснилось опытным путём, лучше поверить на слово
                Sn = parsedOutput[8];
                Console.WriteLine($"Here unconfigured ONU sn: {Sn}");
                //Парсим второй раз, чтобы заполнить номера важные
                parsedOutput = parsedOutput[7].Split(new char[] { '/', '_', ':' }, StringSplitOptions.RemoveEmptyEntries);
                OltNum = Int32.Parse(parsedOutput[1]);
                ShelfNum = Int32.Parse(parsedOutput[2]);
                OnuIfNum = Int32.Parse(parsedOutput[3]);
            }
        }

        public void RegOnu()
        {
            //Внезапно может понадобиться при большом кол-ве зареганых ону
            telnet.WriteLine("terminal length 0");

            //Выводим все зареганные ону, чтобы определить следующую свободную
            string showGponBaseInfo = "show gpon onu baseinfo gpon-olt_" + OltNum + "/" + ShelfNum + "/" + OnuIfNum;
            telnet.WriteLine(showGponBaseInfo);
            string output = telnet.Read();

            //Регулярочкой ищем номера
            MatchCollection regexp = Regex.Matches(output, @":(\d+)");
            List<int> onuIdList = new List<int>();
            foreach( Match m in regexp)
            {
                //Плохая строчка, переделать
                onuIdList.Add(Int32.Parse(m.Groups[1].ToString()));
            }

            List<int> freeOnuIdList = new List<int>();
            bool freeSlotFlag = false;

            // перебираем лист со всеми отображаемые слотами 
            for (int i = 0; i < onuIdList.Count; i++)
            {

                // создаем вспомгательную переменную для записи разницы между проверяемым занятым  слотом и последующим занятым слотом
                int temp = 0;

                // првоерка чтобы при записи разницы в переменную temp не выйти за границу листа
                if (i < onuIdList.Count - 1)
                {
                    temp = onuIdList[i + 1] - onuIdList[i];

                }

                // записываем в лист свободных слотов свободные слоты согласно разницы
                if (temp > 1)
                {
                    for (int j = 1; j < temp; j++)
                    {
                        freeOnuIdList.Add(onuIdList[i] + j);
                        freeSlotFlag = true;
                    }
                }
            }

            //Если пропущенных онушек нет, берём следующую после последней, но меньше 128
            if (!freeSlotFlag)
            {
                if (onuIdList.Count + 1 < 128)
                {
                    OnuId = onuIdList.Count + 1;
                }
                else if (onuIdList.Count + 1 == 128)
                {
                    Console.WriteLine("Warrning, only 1 slot remain.");
                    OnuId = onuIdList.Count + 1;
                }
                else
                {
                    Console.WriteLine("No space for new ONU!");
                    telnet.WriteLine("exit");
                }
            }
            else
                //Если нашли пропущенную
                OnuId = freeOnuIdList[0];
            Console.Write("Type ONU description: ");
            string onuDescr = Console.ReadLine();
            Console.WriteLine("\nTrying register ONU with followed setting:");
            Console.WriteLine($"ONU if: gpon-onu_{OltNum}/{ShelfNum}/{OnuIfNum}:{OnuId} description - {onuDescr}");
            Console.WriteLine($"ONU profiles: line - {LineProfileName}, remote - {RemoteProfileName}");

            telnet.WriteLine("Configure terminal");
            telnet.WriteLine($"interface gpon-olt_{OltNum}/{ShelfNum}/{OnuIfNum}");
            telnet.WriteLine($"onu {OnuId} type F601 sn {Sn}");
            telnet.WriteLine($"onu {OnuId} profile line {LineProfileName} remote {RemoteProfileName}");
            telnet.WriteLine("exit");
            Console.WriteLine(telnet.Read());
            telnet.WriteLine($"interface gpon-onu_{OltNum}/{ShelfNum}/{OnuIfNum}:{OnuId}");
            Console.WriteLine(telnet.Read());
            //В зависимости от версии прошивки регаем ону
            if (FirmwareVer == 1)
            {
                telnet.WriteLine("switchport mode hybrid vport 1");
                Console.WriteLine(telnet.Read());
                telnet.WriteLine($"switchport vlan 110,{RemoteProfileName}  tag vport 1");
                Console.WriteLine(telnet.Read());
                telnet.WriteLine("port-location format flexible-syntax vport 1");
                telnet.WriteLine($"description {onuDescr}");
                telnet.WriteLine($"name {onuDescr}");
                Console.WriteLine("We did it!");
                telnet.WriteLine("exit");
                telnet.WriteLine("exit");
                Console.WriteLine($"show running-config interface gpon-onu_{OltNum}/{ShelfNum}/{OnuIfNum}:{OnuId}");
                telnet.WriteLine("exit");
            } else if (FirmwareVer == 2)
            {
                telnet.WriteLine($"service-port 3 vport 1 user-vlan {RemoteProfileName} vlan {RemoteProfileName}");
                telnet.WriteLine($"description {onuDescr}");
                telnet.WriteLine($"name {onuDescr}");
                Console.WriteLine("We did it!");
                telnet.WriteLine("exit");
                telnet.WriteLine("exit");
                Console.WriteLine($"show running-config interface gpon-onu_{OltNum}/{ShelfNum}/{OnuIfNum}:{OnuId}");
                telnet.WriteLine("exit");
            }
        }
    }
}
