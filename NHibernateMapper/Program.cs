namespace NHibernateMapper
{
    class Program
    {
        private static readonly string inputFilePath = @"..\..\..\IO\Input.txt";
        private static readonly string outputFilePath = @"..\..\..\IO\Output.txt";

        static void Main(string[] args)
        {
            //TODO: Add MappingOptions (PropertyName: underscore to TitleCase etc.)
            try
            {
                MappingHelper.Map(inputFilePath, outputFilePath);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
