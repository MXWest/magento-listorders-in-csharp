using System;
using System.Collections;

class ListOrders {
	private static string _apiUser;
	private static string _apiKey;
	private static string _status;
	private static bool _wantXml = true;
	private static bool _wantHumanReadable = false; 
	private static bool _beSecure = false;
	private static string sep = "------------------------------------------------------\n";
	private static directoryRegionEntity[] _stateCodes;

	private static System.Net.Security.RemoteCertificateValidationCallback mIgnoreInvalidCertificates;

	public static void Main ( string [] args ) {
		mIgnoreInvalidCertificates = new System.Net.Security.RemoteCertificateValidationCallback( delegate { return true; }); 
		MagentoService mageService = new MagentoService();
		string mageSession = null;
		if( args.Length < 3 ) {
			Console.WriteLine("Usage; ListOrders apiUser apiKey status [processing|complete|pending]");
			return;
		}

		try {
			_apiUser = args[0];
			_apiKey = args[1];
			_status = args[2];
			Console.WriteLine( "Connecting to " + mageService.Url );
			if( _beSecure ) {   //require secure communications
				System.Net.ServicePointManager.ServerCertificateValidationCallback -= mIgnoreInvalidCertificates;
				Console.WriteLine("Requiring Valid Certificates from Remote Sites");
			}
			else {   /// Allow connections to SSL sites that have unsafe certificates.
				System.Net.ServicePointManager.ServerCertificateValidationCallback += mIgnoreInvalidCertificates;
				Console.WriteLine("Ignoring Invalid Certificates from Remote Sites");
			}
			mageSession = mageService.login(_apiUser, _apiKey);
		}
		catch( Exception ex ) {
			Console.WriteLine("Login failed: \"" + ex.Message + "\"\n" );
			return;
		}

		try {
			_stateCodes = mageService.directoryRegionList(mageSession, "US");

			salesOrderListEntity[] salesOrders = mageService.salesOrderList(mageSession, tanMageFilter("status", "eq", _status, "10") );

			if( _wantXml ) {
				System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(salesOrders.GetType());
				xml.Serialize(Console.Out,salesOrders);
				Console.WriteLine();
			}

			foreach( salesOrderListEntity orderHeader in salesOrders ) {

				salesOrderEntity orderDetail = mageService.salesOrderInfo(mageSession, orderHeader.increment_id);

    			mageService.salesOrderAddComment(mageSession, orderHeader.increment_id, orderHeader.status, "Examined by remote API", "false");

				if( _wantXml ) {
					System.Xml.Serialization.XmlSerializer xmlDetail = new System.Xml.Serialization.XmlSerializer(orderDetail.GetType());
					xmlDetail.Serialize(Console.Out,orderDetail);
					Console.WriteLine();
				}
				if( _wantHumanReadable ) {
					Console.WriteLine(
						"OrderID  " + orderHeader.increment_id  + "\n"
						+ "OrderDate " + orderHeader.created_at + "\n"
						+ "Status    " + orderHeader.status + "\n"
						+ "SoldTo    " + orderHeader.billing_firstname + " " + orderHeader.billing_lastname + "\n"
						+ "Total     " + orderHeader.grand_total
					);

					salesOrderAddressEntity billToAddress = orderDetail.billing_address;
					Console.WriteLine(
						"BillTo\n" 
						+ "\t" + orderHeader.billing_firstname + " " + orderHeader.billing_lastname + "\n"
						+ "\t" + billToAddress.street + "\n"
						+ "\t" + billToAddress.city + ", " + getStateAbbreviation(billToAddress.region_id) + " " + billToAddress.postcode
					);

					salesOrderAddressEntity shipToAddress = orderDetail.shipping_address;
					Console.WriteLine(
						"ShipTo\n" 
						+ "\t" + orderHeader.shipping_firstname + " " + orderHeader.shipping_lastname + "\n"
						+ "\t" + shipToAddress.street + "\n"
						+ "\t" + shipToAddress.city + ", " + getStateAbbreviation(shipToAddress.region_id) + " " + shipToAddress.postcode
					);

					foreach( salesOrderItemEntity li in orderDetail.items ) {
						Console.WriteLine( li.sku + " " + li.name );
						phpDeserializer(li.product_options);
					}
					Console.WriteLine(sep);
				}
			}
			mageService.endSession(mageSession);
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

	private static filters tanMageFilter( string mageField, string mageOperator, string mageValue, string limit )
	{
		complexFilter myComplexFilter = new complexFilter();
		myComplexFilter.key = mageField;
		myComplexFilter.value = new associativeEntity {
			key = mageOperator, 
			value = mageValue 
		};

		complexFilter myLimitFilter = new complexFilter();
		myLimitFilter.key = "collection.limit";
		myLimitFilter.value = new associativeEntity {
			key = "eq", 
			value = limit 
		};

		filters mageFilters = new filters();
		mageFilters.complex_filter = new complexFilter[] {
			myComplexFilter,
			myLimitFilter
		};
		return mageFilters;
	}

	private static string getStateAbbreviation(string region_id) {
		foreach( directoryRegionEntity _stateCode in _stateCodes ) {
			if( int.Parse(_stateCode.region_id) == int.Parse(region_id) ) {
				return _stateCode.code;
			}
		}
		return "";
	}
}
