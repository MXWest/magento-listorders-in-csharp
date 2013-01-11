using System;
using System.Collections;
using System.Collections.Generic;

class ListOrders {
	private static string _apiUser;
	private static string _apiKey;
	private static string _orderToInvoice;
	private static bool _wantXml = true;
	private static bool _beSecure = false;

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
			_orderToInvoice = args[2];
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
			salesOrderEntity orderDetail = mageService.salesOrderInfo(mageSession, _orderToInvoice);
			List<orderItemIdQty> invoiceInfo = new List<orderItemIdQty>();
			foreach( salesOrderItemEntity li in orderDetail.items ) {
				Console.WriteLine( "Item:" + li.item_id + " Qty:" + li.qty_ordered + " (" + li.name + ")" );
				orderItemIdQty invoiceLineItem = new orderItemIdQty();
				invoiceLineItem.order_item_id = Convert.ToInt32(li.item_id);
				invoiceLineItem.qty = Convert.ToDouble(li.qty_ordered);
				invoiceInfo.Add(invoiceLineItem);
				if( _wantXml ) {
					System.Xml.Serialization.XmlSerializer xmlLineItemDetail =
						new System.Xml.Serialization.XmlSerializer(li.GetType());
					xmlLineItemDetail.Serialize(Console.Out,li);
					Console.WriteLine();
				} 
			}
			orderItemIdQty[] invoiceQtys = invoiceInfo.ToArray();

			/* Create an invoice, and then capture it. Although we are reporting errors, 
				we don't do anything about them, Nor do we stop processing.
			 */
			try {
				string invoiceIncrementId = mageService.salesOrderInvoiceCreate(mageSession, _orderToInvoice,
						invoiceQtys, "Invoiced via API", "1", "1");
				try {
					mageService.salesOrderInvoiceCapture(mageSession, invoiceIncrementId );
				}
				catch( Exception ex ) {
					Console.WriteLine("Invoice Capture Error: \"" + ex.Message + "\"" );
				}
			}
			catch( Exception ex ) {
				Console.WriteLine("Invoice Create Error: \"" + ex.Message + "\"" );

			}

			// Create the shipment, and add tracking for it. Similar to invoicing, we don't stop for errors.
			try {
				string shipmentIncrementId =  mageService.salesOrderShipmentCreate( mageSession, _orderToInvoice, invoiceQtys,
						"Shipment via API", 1, 1 );

    			associativeEntity[] validCarriers = mageService.salesOrderShipmentGetCarriers(mageSession, _orderToInvoice );
				if( _wantXml ) {
					System.Xml.Serialization.XmlSerializer xmlValidCarriers =
						new System.Xml.Serialization.XmlSerializer(validCarriers.GetType());
					xmlValidCarriers.Serialize(Console.Out,validCarriers);
					Console.WriteLine();
				}
				try {
					mageService.salesOrderShipmentAddTrack(mageSession, shipmentIncrementId, "ups", "Schiff UPS Shipping", "1Z9999999999999999");
				}
				catch( Exception ex ) {
					Console.WriteLine("Add Shipment Tracking Error: \"" + ex.Message + "\"" );
				}
			}
			catch( Exception ex ) {
				Console.WriteLine("Shipment Create Error: \"" + ex.Message + "\"" );
			}

			/* Assuming everything went well (which we do ASSume), this order has been Imported into ERP, Invoiced, 
				Captured, and Shipped Complete.
			 */
			mageService.salesOrderAddComment(mageSession, _orderToInvoice, "complete", "Order Completed via API", "0");
			mageService.endSession(mageSession);
		}
		catch( Exception ex ) {
			Console.WriteLine("Error: \"" + ex.Message + "\"\n" );
		}
	}
}
