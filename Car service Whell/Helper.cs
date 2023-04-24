using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Car_service_Whell
{
	public class Helper
	{
		private static Model.Entities DBEntities;
		public static Model.Entities getContex()
		{
			if (DBEntities == null)
			{
				DBEntities = new Model.Entities();
			}
			return DBEntities;
		}

		//получение следующего ID в таблице Order
		public static int getOrderID()
        {
			int id = 0;

			try
			{
				id = DBEntities.Order.OrderByDescending(contract => contract.OrderID).First().OrderID;
				return id + 1;
			}
			catch (InvalidOperationException)
			{
				id = 0;
				return id + 1;
			}
		} 
	}
}
