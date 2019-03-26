using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIClient.ReadReferenceDataSync
{
  class Program
  {
    static void Main(string[] args)     
    {
      //################################################
      //# Set up for Read Reference Data (Synchronous) #
      //################################################
      Console.WriteLine($"Read Reference Data (Synchronous)");

      // Set up client product information (PCIN)
      // Values below should be provided by Medicare
      var product = new nehta.mcaR32.ProviderReadReferenceData.ProductType()
      {
        platform = "Windows 8",     // Can be any value
        productName = "HIAgent",    // Provided by Medicare
        productVersion = "1.0.0",   // Provided by Medicare
        vendor = new nehta.mcaR32.ProviderReadReferenceData.QualifiedId()
        {
          id = "NEHTA001",          // Provided by Medicare               
          qualifier = "http://ns.electronichealth.net.au/id/hi/vendorid/1.0" // Provided by Medicare
        }
      };

      // Set up user identifier details
      var user = new nehta.mcaR32.ProviderReadReferenceData.QualifiedId()
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
      var hpio = new nehta.mcaR32.ProviderReadReferenceData.QualifiedId()
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
      var client = new Nehta.VendorLibrary.HI.ProviderReadReferenceDataClient(
          new Uri(HiServiceEndpoint),
          product,
          user,
          hpio,
          signingCert,
          tlsCert);


      Console.WriteLine($"Submitting request.");
      try
      {
        // Invokes the read operation
        var response = client.ReadReferenceData(new string[]
                            {
                                      "providerTypeCode",
                                      "providerSpecialty",
                                      "providerSpecialisation",
                                      "organisationTypeCode",
                                      "organisationService",
                                      "organisationServiceUnit",
                            });

        //Here is the raw Soap Request and Response
        var RequestSoap = client.SoapMessages.SoapRequest;
        var ResponseSoap = client.SoapMessages.SoapResponse;


        Console.WriteLine($"");
        Console.WriteLine($"HI Service retrieved responses:");
        Console.WriteLine($"----------------------------------------------------------");
        if (response != null && response.readReferenceDataResult != null && response.readReferenceDataResult.elementReferenceValues != null)
        {
          Console.WriteLine($"####################################################################");
          foreach (var elementReferenceValue in response.readReferenceDataResult.elementReferenceValues)
          {            
            Console.WriteLine($" Element Name: {elementReferenceValue.elementName}");
            Console.WriteLine($"####################################################################");
            foreach (var referenceSet in elementReferenceValue.referenceSet)
            {
              Console.WriteLine($"Code: {referenceSet.referenceCode}, Description: {referenceSet.referenceDescription}");              
            }
            Console.WriteLine($"####################################################################");
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
