using System;
using System.Collections.Generic;

namespace c320_onu_reg
{
    class Program
    {
        static void Main(string[] args)
        {

            //Рассказываем юзеру что он тут делает
            Console.WriteLine("Hi! You here for register new ONU. \nInsert commutator number from list bellow. \nPress q for quit or n for add new");

            //Получаем данные из хранилища
            Data data = new Data();
            List<Commutator> commutators;

            //Цикл в котором мы и работаем
            do
            {
                //Получаем список добавленных коммутаторов
                List<Commutator> Commutators = data.GetCommutators();

                //Перебираем список и выводим пользователю
                Console.WriteLine("List:");
                Commutators.ForEach(c => Console.WriteLine($"{Commutators.IndexOf(c)}) {c.Name}"));

                //Получаем ввод от юзера
                Console.Write("Input: ");
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine("");

                //На основании ввода решаем что делать
                switch(key.KeyChar) 
                {
                    case 'q':
                        Environment.Exit(0);
                        break;
                    case 'n':
                        try
                        {
                            data.AddCommutator();
                        } 
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        break;
                    default:
                        try
                        {
                            int commutatorNum = Int32.Parse(key.KeyChar.ToString());
                            commutators = data.GetCommutators();
                            if (commutatorNum > commutators.Count - 1)
                            {
                                Console.WriteLine("Type right commutator number!");
                                break;
                            }
                            Commutators[commutatorNum].Auth();
                            Commutators[commutatorNum].GetUncfgOnu();
                            Commutators[commutatorNum].RegOnu();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            continue;
                        }
                        break;
                }
            } while (true);
        }
    }
}
