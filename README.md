#Magento SOAP API v2: ListOrders with C#/.Net/ Mono#

##About##
I wrote this as a proof of concept for a C# programmer who was new to Magento, not .Net. I have next to Zero Experience in C#, so forgive me if I miss some C# idioms and subtleties. I think if you know .Net/C#, this should be a suitable starting point for you.

##Tools##
Since we're OpenSourcey and Appleish around here, we use [MonoDevelop](http://monodevelop.com/).

Because the SOAP API returns some data in php-serialized format, we also need the [Conversive Sharp Serialization Library](http://csphpserial.sourceforge.net/). Sure glad they wrote that.

I've included a little makefile here, so you can make it. make itself is hardly required, though.

##Setup##
### Know Your Magento API URL##
It will be something like this `http://Your_Magento_Path/index.php/api/v2_soap/?wsdl`

### Know Where your PHPSerializationLibrary.dll Is###
You need to compile against it. I copied a version right into my working directory, so this setup assumes you've done the same.

###Grab the wsdl for consumption by the `wsdl` program###

```
wget -O MagentoService.xml "http://Your_Magento_Path/index.php/api/v2_soap/?wsdl"
```

###Create the .cs file from the wsdl file you just grabbed###

```
wsdl MagentoService.xml
```

###Create the Library (.dll)###

```
gmcs /target:library MagentoService.cs -r:System.Web.Services
```

###Compile the program###

```
mcs /r:System.Web.Services /r:MagentoService.dll /r:PHPSerializationLibrary.dll ListOrders.cs
```

##Running the Program##
###Know Your Magento API User Credentials###
These are the Web Services User / API Key you created in Magento. If you don't know what this means, you'll have to get these from your Magento admin, or learn yourself. Ecommerce Developer has a good, short-n-sweet intro [here](http://ecommercedeveloper.com/articles/2999-make-basic-soap-calls-to-the-magento-core-api/).

###Run It###

```mono ListOrders.exe apiUser apiKey```
