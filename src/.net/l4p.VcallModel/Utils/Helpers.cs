namespace l4p.VcallModel.Utils
{
    interface IHelpers { }

    class HelpersInUse : IHelpers
    {
        private static readonly IHelpers Helpers = new HelpersInUse();

        public static IHelpers All
        {
            get { return Helpers; }
        }
    }
}