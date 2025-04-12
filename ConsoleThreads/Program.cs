namespace ConsoleThreads;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("[INFO] Prod started");
        List<Object> objects = new List<Object>
        {
            2314,
            "hello python"
        };
        
        PrintObjects(objects);
    }


    static void PrintObjects(List<Object> objects)
    {
        try
        {
            if (objects.Count > 0)
            {
                foreach (var object_item in objects)
                {
                    Console.WriteLine(object_item.ToString());
                }
            }
            else
            {
                Console.WriteLine("There is nothing to print");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}