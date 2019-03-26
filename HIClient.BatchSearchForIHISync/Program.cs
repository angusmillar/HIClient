using System;
using System.Collections.Generic;
using System.Linq;
using Nehta.VendorLibrary.HI.Common;

namespace HIClient.BatchSearchForIHISync
{
  class Program
  {
    static void Main(string[] args)
    {
      //###############################################
      //#  Set up for IHI Batch Search (Synchronous)  #
      //###############################################
      Console.WriteLine($"IHI Batch Search (Synchronous)");
      // Set up client product information (PCIN)
      // Values below should be provided by Medicare
      var product = new nehta.mcaR3.ConsumerSearchIHIBatchSync.ProductType()
      {
        platform = "Windows 8",     // Can be any value
        productName = "HIAgent",    // Provided by Medicare
        productVersion = "1.0.0",   // Provided by Medicare
        vendor = new nehta.mcaR3.ConsumerSearchIHIBatchSync.QualifiedId()
        {
          id = "NEHTA001",          // Provided by Medicare               
          qualifier = "http://ns.electronichealth.net.au/id/hi/vendorid/1.0" // Provided by Medicare
        }
      };

      // Set up user identifier details
      var user = new nehta.mcaR3.ConsumerSearchIHIBatchSync.QualifiedId()
      {
        id = "TestUserId", // User ID internal to your system
        qualifier = "http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0" // Eg: http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0
      };

      // Set up the calling organisation HPI-O identifier. 
      // Only required if interacting as a Contracted Service Provider CSP
      // If used, supplied to the client as the 4th parameter.
      var hpio = new nehta.mcaR3.ConsumerSearchIHIBatchSync.QualifiedId()
      {
        id = "[Calling organisation's HPI-O] ",                   // HPIO internal to your system
        qualifier = "http://<anything>/id/<anything>/hpio/1.0"    // Eg: http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0
      };

      // Obtain the HI Service certificate by serial number from the windows local machine certificate personal store 
      System.Security.Cryptography.X509Certificates.X509Certificate2 tlsCert = Nehta.VendorLibrary.Common.X509CertificateUtil.GetCertificate(
          "0608d9",
          System.Security.Cryptography.X509Certificates.X509FindType.FindBySerialNumber,
          System.Security.Cryptography.X509Certificates.StoreName.My,
          System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
          true
          );

      //The same certificate is used for signing the SOAP request. 
      //This certificate will be different to TLS cert for some operations.
      System.Security.Cryptography.X509Certificates.X509Certificate2 signingCert = tlsCert;

      // Instantiate the client
      //HiServiceTestEndpoint = "https://www5.medicareaustralia.gov.au/cert/soap/services/";
      //HiServiceProdEndpoint = "https://www3.medicareaustralia.gov.au/pcert/soap/services/";
      string HiServiceEndpoint = "https://www5.medicareaustralia.gov.au/cert/soap/services/";
      var client = new Nehta.VendorLibrary.HI.ConsumerSearchIHIBatchSyncClient(
          new Uri(HiServiceEndpoint),
          product,
          user,
          hpio,
          signingCert,
          tlsCert);

      // Create a list of search requests
      var searches = new List<Nehta.VendorLibrary.HI.Common.CommonSearchIHIRequestType>();


      // Add a basic search
      searches.AddBasicSearch(Guid.NewGuid().ToString(), new Nehta.VendorLibrary.HI.Common.CommonSearchIHI()
      {
        ihiNumber = Nehta.VendorLibrary.Common.HIQualifiers.IHIQualifier + "8003601240022579",
        dateOfBirth = DateTime.Parse("12 Dec 2002"),
        givenName = "Jessica",
        familyName = "Wood",
        sex = Nehta.VendorLibrary.HI.Common.CommonSexType.F
      });

      // Add a basic Medicare search
      searches.AddBasicMedicareSearch(Guid.NewGuid().ToString(), new Nehta.VendorLibrary.HI.Common.CommonSearchIHI()
      {
        medicareCardNumber = "2950141861",
        medicareIRN = "1",
        dateOfBirth = DateTime.Parse("12 Dec 2002"),
        givenName = "Jessica",
        familyName = "Wood",
        sex = Nehta.VendorLibrary.HI.Common.CommonSexType.F
      });

      // Add a basic DVA search
      searches.AddBasicDvaSearch(Guid.NewGuid().ToString(), new Nehta.VendorLibrary.HI.Common.CommonSearchIHI()
      {
        dvaFileNumber = "N 908030C",
        dateOfBirth = DateTime.Parse("12 Dec 1970"),
        givenName = "Luke",
        familyName = "Lawson",
        sex = Nehta.VendorLibrary.HI.Common.CommonSexType.M
      });

      // Add an Australian street address search
      searches.AddAustralianStreetAddressSearch(Guid.NewGuid().ToString(), new Nehta.VendorLibrary.HI.Common.CommonSearchIHI()
      {
        dateOfBirth = DateTime.Parse("12 Dec 2002"),
        givenName = "Jessica",
        familyName = "Wood",
        sex = Nehta.VendorLibrary.HI.Common.CommonSexType.F,
        australianStreetAddress = new Nehta.VendorLibrary.HI.Common.CommonAustralianStreetAddressType()
        {
          streetNumber = "21",
          streetName = "Ross",
          streetType = Nehta.VendorLibrary.HI.Common.CommonStreetType.RD,
          streetTypeSpecified = true,
          suburb = "Queanbeyan",
          state = Nehta.VendorLibrary.HI.Common.CommonStateType.NSW,
          postcode = "2620"
        }
      });

      try
      {
        Console.WriteLine("==================================================================");
        Console.WriteLine("=== HI Service Search IHI Batch Sync =============================");
        Console.WriteLine("==================================================================");
        Console.WriteLine("Sending Synchronous batch, please wait for batch response");
        nehta.mcaR3.ConsumerSearchIHIBatchSync.searchIHIBatchResponse ihiResponse = client.SearchIHIBatchSync(searches);

        //Here is the raw Soap Request and Response
        var RequestSoap = client.SoapMessages.SoapRequest;
        var ResponseSoap = client.SoapMessages.SoapResponse;

        Console.WriteLine("Response Received.");
        Console.WriteLine($"--------------------------------------------------");
        foreach (Nehta.VendorLibrary.HI.Common.CommonSearchIHIRequestType request in searches)
        {
          //Get the response for each request by requestIdentifier
          var target = ihiResponse.searchIHIBatchResult.SingleOrDefault(x => x.requestIdentifier == request.requestIdentifier);
          Console.WriteLine($"Patient:{target.searchIHIResult.familyName}, {target.searchIHIResult.givenName} ");
          Console.WriteLine($"IHI Number:{target.searchIHIResult.ihiNumber}");
          Console.WriteLine($"IHI Record Status:{target.searchIHIResult.ihiRecordStatus.ToString()}");
          Console.WriteLine($"IHI Status:{target.searchIHIResult.ihiRecordStatus.ToString()}");
          Console.WriteLine($"--------------------------------------------------");
        }
        Console.WriteLine("Hit any key to end.");
        Console.ReadKey();
      }
      catch (System.ServiceModel.FaultException fex)
      {
        Console.WriteLine("==================================================================");
        Console.WriteLine("=== FaultException ===============================================");
        Console.WriteLine("==================================================================");
        string returnError = "";
        System.ServiceModel.Channels.MessageFault fault = fex.CreateMessageFault();
        if (fault.HasDetail)
        {
          nehta.mcaR3.ConsumerSearchIHIBatchSync.ServiceMessagesType error = fault.GetDetail<nehta.mcaR3.ConsumerSearchIHIBatchSync.ServiceMessagesType>();
          // Look at error details in here
          if (error.serviceMessage.Length > 0)
          {
            returnError = error.serviceMessage[0].code + ": " + error.serviceMessage[0].reason;
            Console.WriteLine($"Service Message: {returnError}");
          }
        }

        // If an error is encountered, client.LastSoapResponse often provides a more
        // detailed description of the error.
        string soapResponse = client.SoapMessages.SoapResponse;
        Console.WriteLine($"Soap Response: {soapResponse}");
        Console.WriteLine("==================================================================");
        Console.WriteLine("Hit any key to end.");
        Console.ReadKey();
      }
      catch (Exception ex)
      {
        // If an error is encountered, client.LastSoapResponse often provides a more
        // detailed description of the error.
        string soapResponse = client.SoapMessages.SoapResponse;
        Console.WriteLine($"Exception: {ex.Message}");
        Console.WriteLine($"Soap Response: {soapResponse}");
        Console.WriteLine("==================================================================");
        Console.WriteLine("Hit any key to end.");
        Console.ReadKey();
      }
    }
  }
}
