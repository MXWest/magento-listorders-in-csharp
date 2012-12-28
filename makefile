#
#
#
ListOrders.exe: ListOrders.cs MagentoService.dll
	mcs \
	/r:System.Web.Services \
	/r:MagentoService.dll  \
	/r:PHPSerializationLibrary.dll \
	ListOrders.cs
MagentoService.dll: MagentoService.cs
	gmcs \
	/target:library MagentoService.cs \
	-r:System.Web.Services
