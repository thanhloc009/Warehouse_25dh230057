using System.Web;
using System.Web.Mvc;

namespace Warehouse_25dh230057
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
