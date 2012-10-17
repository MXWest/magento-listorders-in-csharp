#
#
#
ListOrders.exe: ListOrders.cs
	mcs \
	/r:System.Web.Services \
	/r:MagentoService.dll  \
	/r:PHPSerializationLibrary.dll \
	ListOrders.cs
