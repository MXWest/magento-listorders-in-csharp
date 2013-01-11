#
#
#
all: ListOrders.exe InvoiceOrder.exe
ListOrders.exe: ListOrders.cs MagentoService.dll
	mcs \
	/r:System.Web.Services \
	/r:MagentoService.dll  \
	/r:PHPSerializationLibrary.dll \
	ListOrders.cs
InvoiceOrder.exe: InvoiceOrder.cs MagentoService.dll
	mcs \
	/r:System.Web.Services \
	/r:MagentoService.dll  \
	InvoiceOrder.cs
MagentoService.dll: MagentoService.cs
	gmcs \
	/target:library MagentoService.cs \
	-r:System.Web.Services
