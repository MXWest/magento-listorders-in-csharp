using System;
using System.Collections;

class ListOrders {
	private static string _apiUser;
	private static string _apiKey;
	private static bool _wantXml = false;
	private static string sep = "------------------------------------------------------\n";

	public static void Main ( string [] args ) {
		MagentoService mageService = new MagentoService();
		string mageSession = null;
		if( args.Length < 2 ) {
			Console.WriteLine("Usage; ListOrders apiUser apiKey");
			return;
		}

		try {
			_apiUser = args[0];
			_apiKey = args[1];
			mageSession = mageService.login(_apiUser, _apiKey);
		}
		catch( Exception ex ) {
			Console.WriteLine("Login failed: \"" + ex.Message + "\"\n" );
			return;
		}

		try {
			salesOrderListEntity[] salesOrders = mageService.salesOrderList(mageSession, tanMageFilter("status", "eq", "processing") );

			if( _wantXml ) {
				System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(salesOrders.GetType());
				xml.Serialize(Console.Out,salesOrders);
				Console.WriteLine();
			}

			foreach( salesOrderListEntity orderHeader in salesOrders ) {
				Console.WriteLine(
					"OrderID  " + orderHeader.increment_id  + "\n"
					+ "OrderDate " + orderHeader.created_at + "\n"
					+ "Status    " + orderHeader.status + "\n"
					+ "SoldTo    " + orderHeader.billing_firstname + " " + orderHeader.billing_lastname + "\n"
					+ "Total     " + orderHeader.grand_total
				);


				salesOrderEntity orderDetail = mageService.salesOrderInfo(mageSession, orderHeader.increment_id);

				if( _wantXml ) {
					System.Xml.Serialization.XmlSerializer xmlDetail = new System.Xml.Serialization.XmlSerializer(orderDetail.GetType());
					xmlDetail.Serialize(Console.Out,orderDetail);
					Console.WriteLine();
				}

				salesOrderAddressEntity billToAddress = orderDetail.billing_address;
				Console.WriteLine(
					"BillTo\n" 
					+ "\t" + orderHeader.billing_firstname + " " + orderHeader.billing_lastname + "\n"
					+ "\t" + billToAddress.street + "\n"
					+ "\t" + billToAddress.city + ", " + billToAddress.region + " " + billToAddress.postcode
				);

				salesOrderAddressEntity shipToAddress = orderDetail.shipping_address;
				Console.WriteLine(
					"ShipTo\n" 
					+ "\t" + orderHeader.shipping_firstname + " " + orderHeader.shipping_lastname + "\n"
					+ "\t" + shipToAddress.street + "\n"
					+ "\t" + shipToAddress.city + ", " + shipToAddress.region + " " + shipToAddress.postcode
				);

				foreach( salesOrderItemEntity li in orderDetail.items ) {
					Console.WriteLine( li.sku + " " + li.name );
					phpDeserializer(li.product_options);
				}
				Console.WriteLine(sep);
			}
		}
		catch( Exception ex ) {
			Console.WriteLine("I was hoping for better than this. Error: \"" + ex.Message + "\"\n" );
		}
	}

	private static void phpDeserializer(string product_options) {
		Conversive.PHPSerializationLibrary.Serializer php = new Conversive.PHPSerializationLibrary.Serializer();
		Hashtable ht = (Hashtable)php.Deserialize(product_options);
		htDumper(ht);
		return;
	}

	private static void htDumper(Hashtable ht) {
		foreach( DictionaryEntry d in ht ) {
			if( d.Value is Hashtable ) {
				Console.WriteLine(d.Key);
				htDumper((Hashtable)d.Value);
			}
			else {
				Console.WriteLine("\t\t{0}=>[{1}]", d.Key, d.Value );
			}
		}
		return;
	}

	private static filters tanMageFilter(string mageField, string mageOperator, string mageValue ) {
		complexFilter myComplexFilter = new complexFilter();
		myComplexFilter.key = mageField;
		myComplexFilter.value = new associativeEntity {
			key = mageOperator, 
			value = mageValue 
		};

		filters mageFilters = new filters();
		mageFilters.complex_filter = new complexFilter[] {
			myComplexFilter
		};
		return mageFilters;
	}
}
