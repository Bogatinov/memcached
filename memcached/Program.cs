using System;
using System.Collections.Generic;
using System.Net;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;

namespace memcached
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (var client = new MemcachedClient())
            //{
            //    client.Store(StoreMode.Set, "currentTime", DateTime.UtcNow.ToString());
            //    string value = client.Get<string>("currentTime");
            //    Console.WriteLine(client.Stats());
            //    Console.WriteLine(value);
            //    Console.ReadKey();
            //}

            // create a MemcachedClient
			// in your application you can cache the client in a static variable or just recreate it every time
			// MemcachedClient mc = new MemcachedClient();

			// you can create another client using a different section from your app/web.config
			// this client instance can have different pool settings, key transformer, etc.
			// MemcachedClient mc2 = new MemcachedClient("memcached");

			// or just initialize the client from code

            MemcachedClientConfiguration config = new MemcachedClientConfiguration();
            config.Servers.Add(new IPEndPoint(IPAddress.Loopback, 11211));
            config.Protocol = MemcachedProtocol.Binary;

			var mc = new MemcachedClient();

			for (var i = 0; i < 100; i++)
				mc.Store(StoreMode.Set, "Hello", "World");

            var myHello = mc.Get("Hello");
            Console.WriteLine(myHello);


			// simple multiget; please note that only 1.2.4 supports it (windows version is at 1.2.1)
            List<string> keys = new List<string>();

            for (int i = 1; i < 100; i++)
            {
                string k = "aaaa" + i + "--" + (i * 2);
                keys.Add(k);

                mc.Store(StoreMode.Set, k, i);
            }

            IDictionary<string, ulong> cas;
            IDictionary<string, object> retvals = mc.Get(keys);

            List<string> keys2 = new List<string>(keys);
            keys2.RemoveRange(0, 50);

            IDictionary<string, object> retvals2 = mc.Get(keys2);
            retvals2 = mc.Get(keys2);

            ServerStats ms = mc.Stats();

			// store a string in the cache
            mc.Store(StoreMode.Set, "MyKey", "Hello World");

			// retrieve the item from the cache
            Console.WriteLine("MyKey: {0}", mc.Get("MyKey"));

            Console.WriteLine("Increment num1 by 10 {0}", mc.Increment("num1", 1, 10));
            Console.WriteLine("Increment num1 by 10 {0}", mc.Increment("num1", 1, 10));
            Console.WriteLine("Increment num1 by 14 {0}", mc.Decrement("num1", 1, 14));

			//// store some other items
            mc.Store(StoreMode.Set, "D1", 1234L);
            mc.Store(StoreMode.Set, "D2", DateTime.Now);
            mc.Store(StoreMode.Set, "D3", true);
            mc.Store(StoreMode.Set, "D4", new Product());

            mc.Store(StoreMode.Set, "D5", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });


            mc.Store(StoreMode.Set, "D1", 1234L);
            mc.Store(StoreMode.Set, "D2", DateTime.Now);
            mc.Store(StoreMode.Set, "D3", true);
            mc.Store(StoreMode.Set, "D4", new Product());

            Console.WriteLine("D1: {0}", mc.Get("D1"));
            Console.WriteLine("D2: {0}", mc.Get("D2"));
            Console.WriteLine("D3: {0}", mc.Get("D3"));
            Console.WriteLine("D4: {0}", mc.Get("D4"));
            Console.WriteLine("D5: {0}", System.Text.Encoding.UTF8.GetString(mc.Get<byte[]>("D5")));

            Console.WriteLine("Removing D1-D4");
            // delete them from the cache
            mc.Remove("D1");
            mc.Remove("D2");
            mc.Remove("D3");
            mc.Remove("D4");

			//ServerStats stats = mc.Stats();
            Console.WriteLine("Active Connections: {0}",ms.GetValue(ServerStats.All, StatItem.ConnectionCount));
            Console.WriteLine("GET operations {0}", ms.GetValue(ServerStats.All, StatItem.GetCount));

			//// add an item which is valid for 10 mins
            mc.Store(StoreMode.Set, "D4", new Product(), new TimeSpan(0, 10, 0));
            Console.WriteLine(mc.Stats().GetValue(new IPEndPoint(IPAddress.Loopback, 11211), StatItem.BytesRead));
			Console.ReadLine();
		}

		// objects must be serializable to be able to store them in the cache
        [Serializable]
        class Product
        {
            public double Price = 1.24;
            public string Name = "Mineral Water";

            public override string ToString()
            {
                return String.Format("Product {{{0}: {1}}}", this.Name, this.Price);
            }
        }
    }
}
