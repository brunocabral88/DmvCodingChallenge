using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DmvAppointmentScheduler
{
    class Program
    {
        private static Random random { get; set; } = new Random();
        private static Dictionary<string, double> tellersTotalTime = new Dictionary<string, double>();
        public static List<Appointment> appointmentList = new List<Appointment>();
        static void Main(string[] args)
        {
            CustomerList customers = ReadCustomerData();
            TellerList tellers = ReadTellerData();

            Calculation(customers, tellers);
            OutputTotalLengthToConsole(appointmentList);
        }
        private static CustomerList ReadCustomerData()
        {
            string fileName = "CustomerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            CustomerList customerData = JsonConvert.DeserializeObject<CustomerList>(jsonString);
            return customerData;

        }
        private static TellerList ReadTellerData()
        {
            string fileName = "TellerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            TellerList tellerData = JsonConvert.DeserializeObject<TellerList>(jsonString);
            return tellerData;

        }
        static void Calculation(CustomerList customers, TellerList tellers)
        {
            // Your code goes here .....
            // Re-write this method to be more efficient instead of a assigning all customers to the same teller

            var naiveMeanTime = customers.Customer.Sum(x => x.duration) / tellers.Teller.Count;
            var currentTellerIndex = 0;
            var threshold = 0.6;

            foreach (var customer in customers.Customer)
            {

                var customerProcessed = false;

                while (!customerProcessed)
                {
                    if (currentTellerIndex >= tellers.Teller.Count)
                    {
                        threshold += 0.1;
                        currentTellerIndex = 0;
                    }

                    var currentTeller = tellers.Teller[currentTellerIndex];
                    var currentTellerTotalTime = tellersTotalTime.ContainsKey(currentTeller.id) ? tellersTotalTime[currentTeller.id] : 0;

                    if (currentTellerTotalTime > naiveMeanTime * threshold)
                    {
                        currentTellerIndex++;
                    }
                    else
                    {
                        var app = new Appointment(customer, currentTeller);
                        appointmentList.Add(app);
                        customerProcessed = true;

                        IncrementTellerTotalTime(currentTeller, app, tellersTotalTime);
                    }
                }
            }
        }

        private static void IncrementTellerTotalTime(Teller teller, Appointment app, Dictionary<string, double> timeCounter)
        {
            if (tellersTotalTime.ContainsKey(teller.id))
            {
                timeCounter[teller.id] += app.duration;
            }
            else
            {
                timeCounter[teller.id] = app.duration;
            }
        }

        static void OutputTotalLengthToConsole(IList<Appointment> appointments)
        {
            var tellerAppointments =
                from appointment in appointments
                group appointment by appointment.teller into tellerGroup
                select new
                {
                    teller = tellerGroup.Key,
                    totalDuration = tellerGroup.Sum(x => x.duration),
                };
            var max = tellerAppointments.OrderBy(i => i.totalDuration).LastOrDefault();
            Console.WriteLine("Teller " + max.teller.id + " will work for " + max.totalDuration + " minutes!");
            Console.ReadKey();
        }

    }
}
