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
                for (int i = 0; i < Commutators.Count; i++)
                {
                    Console.WriteLine($"{i}) {Commutators[i].Name}");
                }

                //Получаем ввод от юзера
                Console.Write("Input: ");
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine("");

                //На основании ввода решаем что делать
                if (key.KeyChar == 'q')
                    break;
                else if (key.KeyChar == 'n')
                {
                    try
                    {
                        data.AddCommutator();
                    } catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
                else
                {
                    int commutatorNum;
                    try
                    {
                        commutatorNum = Int32.Parse(key.KeyChar.ToString());
                    }
                    catch
                    {
                        Console.WriteLine("Wrong input! Only commutator number/q/n allowed.");
                        continue;
                    }
                    commutators = data.GetCommutators();
                    if (commutatorNum > commutators.Count - 1)
                    {
                        Console.WriteLine("Type right commutator number!");
                        continue;
                    }
                    Commutators[commutatorNum].Auth();
                    try
                    {
                        Commutators[commutatorNum].GetUncfgOnu();
                    } catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    Commutators[commutatorNum].RegOnu();
                }
            } while (true);
        }
    }
}
