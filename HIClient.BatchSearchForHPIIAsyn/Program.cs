using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIClient.BatchSearchForHPIIAsyn
{
  class Program
  {
    static void Main(string[] args)    
    {
      //###############################################
      //# Set up for HPI-I Batch Search (Asynchronous)#
      //###############################################
      Console.WriteLine($"HPI-I Batch Search (Asynchronous)");
      // Set up client product information (PCIN)
      // Values below should be provided by Medicare
      var product = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.ProductType()
      {
        platform = "Windows 8",     // Can be any value
        productName = "HIAgent",    // Provided by Medicare
        productVersion = "1.0.0",   // Provided by Medicare
        vendor = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.QualifiedId()
        {
          id = "NEHTA001",          // Provided by Medicare               
          qualifier = "http://ns.electronichealth.net.au/id/hi/vendorid/1.0" // Provided by Medicare
        }
      };

      // Set up user identifier details
      var user = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.QualifiedId()
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
      var hpio = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.QualifiedId()
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
      var client = new Nehta.VendorLibrary.HI.ProviderBatchAsyncSearchForProviderIndividualClient(
          new Uri(HiServiceEndpoint),
          product,
          user,
          null,
          signingCert,
          tlsCert);


      // Create a Batch/List to be populated with each individual HPI-I search details 
      var SearchBatchList = new List<nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.BatchSearchForProviderIndividualCriteriaType>();

      //Add the 1st entry to the batch (AHPRA Registration number)
      SearchBatchList.Add(new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.BatchSearchForProviderIndividualCriteriaType()
      {
        //This is the minimum HPI-I search consisting of a AHPRA Registration number 'TES0000000001' and Family name 'Ellis'   
        requestIdentifier = Guid.NewGuid().ToString(),
        searchForProviderIndividual = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.searchForProviderIndividual()
        {
          //The 'registrationId' is the AHPRA Registration Number
          //You can lookup an individual's AHPRA Registration Numbers on 
          //the AHPRA web site here: https://www.ahpra.gov.au/registration/registers-of-practitioners.aspx
          registrationId = "TES0000000001",
          familyName = "Ellis",
        }
      });

      //Add a 2nd entry to the batch (AHPRA Registration number with extra demographics)
      SearchBatchList.Add(new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.BatchSearchForProviderIndividualCriteriaType()
      {
        //Provide more demographic information for more accurate matching
        requestIdentifier = Guid.NewGuid().ToString(),
        searchForProviderIndividual = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.searchForProviderIndividual()
        {          
          registrationId = "TES0000000002",
          givenName = new string[] { "Percy" },
          familyName = "Chappell",
          dateOfBirth = DateTime.Parse("21 Oct 1965"),
          sex = nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.SexType.F,          
        }
      });

      //Add a 3rd entry to the batch (HPI-I Search primarily to check the HPI-I is still active)
      SearchBatchList.Add(new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.BatchSearchForProviderIndividualCriteriaType()
      {
        //Search by IHI and family name, can be used to confirm the status of the HPI-I is still active 
        requestIdentifier = Guid.NewGuid().ToString(),
        searchForProviderIndividual = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.searchForProviderIndividual()
        {
          hpiiNumber = Nehta.VendorLibrary.Common.HIQualifiers.HPIIQualifier + "8003613233362573",
          familyName = "Holmes"
        }
      });
      
      //Add a 4th entry to the batch (Address & demographics search)
      SearchBatchList.Add(new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.BatchSearchForProviderIndividualCriteriaType()
      {
        //When performing an address search you can not supply an identifier (AHPRA or HPI-I)
        requestIdentifier = Guid.NewGuid().ToString(),
        searchForProviderIndividual = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.searchForProviderIndividual()
        {          
          familyName = "BURNETTE",
          givenName = new string[] { "BEVAN" },
          dateOfBirth = DateTime.Parse("14 Jun 1968"),
          sex = nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.SexType.M,
          dateOfBirthSpecified = true,
          sexSpecified = true,
          searchAustralianAddress = new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.SearchAustralianAddressType()
          {
            streetNumber = "10",
            streetName = "HORACIO GR",
            suburb = "PORT CAMPBELL",
            postcode = "3269",
            state = nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.StateType.VIC,
          }
        }
      });
      Console.WriteLine($"Creating request batch with {SearchBatchList.Count.ToString()} entries.");

      try
      {
        Console.WriteLine($"Submitting batch to HI Service, please wait.");
        var submitResponse = client.BatchSubmitProviderIndividuals(SearchBatchList.ToArray());

        //Here is the raw Soap Request and Response
        var RequestSoap = client.SoapMessages.SoapRequest;
        var ResponseSoap = client.SoapMessages.SoapResponse;

        Console.WriteLine($"HI Service response Message: {submitResponse.submitSearchForProviderIndividualResult.serviceMessages.serviceMessage[0].reason}");

        // Retrieve the batch result
        Console.WriteLine($"Retrieve asynchronous response, please wait. ");
        var retrieveResponse = client.BatchRetrieveProviderIndividuals(new nehta.mcaR51.ProviderBatchAsyncSearchForProviderIndividual.retrieveSearchForProviderIndividual()
        {
          batchIdentifier = submitResponse.submitSearchForProviderIndividualResult.batchIdentifier
        });
        
        Console.WriteLine($"");
        Console.WriteLine($"HI Service retrieved responses:");
        Console.WriteLine($"----------------------------------------------------------");
        foreach (var SearchRequest in SearchBatchList)
        {
          var Target = retrieveResponse.retrieveSearchForProviderIndividualResult.batchSearchForProviderIndividualResult.SingleOrDefault(x => x.requestIdentifier == SearchRequest.requestIdentifier);                    
          Console.WriteLine($"Family name: {Target.searchForProviderIndividualResult.familyName}");
          Console.WriteLine($"HPI-I number: {Target.searchForProviderIndividualResult.hpiiNumber}");
          Console.WriteLine($"HPI-I status: {Target.searchForProviderIndividualResult.status}");
          Console.WriteLine($"----------------------------------------------------------");
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
