using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIClient.SearchForHPDIndividual
{
  class Program
  {
    static void Main(string[] args)    
    {
      //###########################################################################################
      //# Set up for Search HI Service's Health Provider Directory data for a provider Individual #
      //###########################################################################################
      Console.WriteLine($"Search HI Service's Health Provider Directory data for a provider Individual");
      // Set up client product information (PCIN)
      // Values below should be provided by Medicare
      var product = new nehta.mcaR32.ProviderSearchHIProviderDirectoryForIndividual.ProductType()
      {
        platform = "Windows 8",     // Can be any value
        productName = "HIAgent",    // Provided by Medicare
        productVersion = "1.0.0",   // Provided by Medicare
        vendor = new nehta.mcaR32.ProviderSearchHIProviderDirectoryForIndividual.QualifiedId()
        {
          id = "NEHTA001",          // Provided by Medicare               
          qualifier = "http://ns.electronichealth.net.au/id/hi/vendorid/1.0" // Provided by Medicare
        }
      };

      // Set up user identifier details
      var user = new nehta.mcaR32.ProviderSearchHIProviderDirectoryForIndividual.QualifiedId()
      {
        id = "TestUserId", // User ID internal to your system
        qualifier = "http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0" // Eg: http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0
      };

      // Set up the calling organisation HPI-O identifier if required. 
      // Only required if interacting as a Contracted Service Provider (CSP)
      // If used, supply this 'hpio' instance to the client below as the 4th parameter. 
      // This must be the HPI-O of the site you are transacting for as part of your Contracted Service Provider role. 
      // This example assumes the use of a Medicare site certificate rather than a Medicare CSP Certificate
      // and therefore supplies null as the 4th parameter on the client below.
      var hpio = new nehta.mcaR32.ProviderSearchHIProviderDirectoryForIndividual.QualifiedId()
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

      //Instantiate the client
      //HiServiceTestEndpoint = "https://www5.medicareaustralia.gov.au/cert/soap/services/";
      //HiServiceProdEndpoint = "https://www3.medicareaustralia.gov.au/pcert/soap/services/";
      string HiServiceEndpoint = "https://www5.medicareaustralia.gov.au/cert/soap/services/";
      // Instantiate the client
      var client = new Nehta.VendorLibrary.HI.ProviderSearchHIProviderDirectoryForIndividualClient(
          new Uri(HiServiceEndpoint),
          product,
          user,
          hpio,
          signingCert,
          tlsCert);

      // Set up the request      
      var request = new nehta.mcaR32.ProviderSearchHIProviderDirectoryForIndividual.searchHIProviderDirectoryForIndividual();
      request.hpiiNumber = Nehta.VendorLibrary.Common.HIQualifiers.HPIIQualifier + "8003613233362417";
      Console.WriteLine($"Submitting request for HPI-I: {request.hpiiNumber}");

      try
      {
        // Invokes the batch search
        var response = client.IdentifierSearch(request);

        //Here is the raw Soap Request and Response
        var RequestSoap = client.SoapMessages.SoapRequest;
        var ResponseSoap = client.SoapMessages.SoapResponse;


        Console.WriteLine($"");
        Console.WriteLine($"HI Service retrieved responses:");
        Console.WriteLine($"----------------------------------------------------------");
        foreach (var Entry in response.searchHIProviderDirectoryForIndividualResult.individualProviderDirectoryEntries)
        {
          Console.WriteLine($"HPI-I Number: {Entry.hpiiNumber}");
          if (Entry.individualName.familyName != null)
            Console.WriteLine($"Family Name: {Entry.individualName.familyName}");
          if (Entry.individualName.givenName != null)
          {
            foreach (var GivenName in Entry.individualName.givenName)
            {
              Console.WriteLine($"Given Name: {GivenName}");
            }
          }
          if (Entry.electronicCommunication != null)
          {
            foreach(var Com in  Entry.electronicCommunication)
            {
              if (!string.IsNullOrWhiteSpace(Com.details))
                Console.WriteLine($"Communication: {Com.details}");
            }
          }
          if (Entry.address != null && Entry.address.australianAddress != null)
          {
             if (!string.IsNullOrWhiteSpace(Entry.address.australianAddress.streetNumber))
               Console.WriteLine($"StreetNumber: {Entry.address.australianAddress.streetNumber}");
            if (!string.IsNullOrWhiteSpace(Entry.address.australianAddress.streetName))
              Console.WriteLine($"StreetName: {Entry.address.australianAddress.streetName}");
            if (!string.IsNullOrWhiteSpace(Entry.address.australianAddress.suburb))
              Console.WriteLine($"Suburb: {Entry.address.australianAddress.suburb}");
            if (!string.IsNullOrWhiteSpace(Entry.address.australianAddress.postcode))
              Console.WriteLine($"Postcode: {Entry.address.australianAddress.postcode}");            
              Console.WriteLine($"State: {Entry.address.australianAddress.state.ToString()}");
          }
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
          nehta.mcaR3.ConsumerSearchIHIBatchAsync.ServiceMessagesType error = fault.GetDetail<nehta.mcaR3.ConsumerSearchIHIBatchAsync.ServiceMessagesType>();
          // Look at error details in here
          if (error.serviceMessage.Length > 0)
          {
            returnError = error.serviceMessage[0].code + ": " + error.serviceMessage[0].reason;
            Console.WriteLine($"Service message: {returnError}");
          }
        }
        // If an error is encountered, client.LastSoapResponse often provides a more
        // detailed description of the error.
        string soapResponse = client.SoapMessages.SoapResponse;
        Console.WriteLine($"Soap response: {soapResponse}");
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
        Console.WriteLine($"Soap response: {soapResponse}");
        Console.WriteLine("==================================================================");
        Console.WriteLine("Hit any key to end.");
        Console.ReadKey();
      }
    }
  }
}
