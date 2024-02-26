using Alloc8_web.ViewModels.ChatGpt;
using Alloc8.ef.Entities.Dashboard;
using Alloc8_web.Models;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Alloc8.ef;
using Alloc8.ef.Entities.Dashboard;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace ActualisationDashboard.Controllers
{
    public class LLMController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private static Alloc8DbContext dbContext;
        public LLMController(Alloc8DbContext _dbContext, UserManager<ApplicationUser> userManager)
        {
            dbContext = _dbContext;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index1()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var userId = string.Empty;
            var user = await _userManager.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefaultAsync();

            if (user != null)
            {
                userId = user.Id;
            }
            if (string.IsNullOrEmpty(userId))
            {
                return View();
            }
          
                var chatSessionGroups = await getAhFenicngChatSessionGroupsAsync();
                return View(chatSessionGroups);
        }
        public async Task<IActionResult> Api(string prompt, string chatId)
        {
            var userId = string.Empty;

            var user = await _userManager.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefaultAsync();

             return await AhFencingOpenApi(prompt, chatId);          
           
        }
        public async Task<IActionResult> GetChatHistory(int chatId)
        {
            var chatHistoryList = dbContext.alloc8ChatGptHistory.Where(x => x.chatSessionId == chatId).ToList();

            return Json(chatHistoryList);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteChatHistory(int chatId)
        {
            try
            {
                var chatHistoryList = dbContext.alloc8ChatGptHistory.Where(x => x.chatSessionId == chatId).ToList();
                foreach (var chatHistory in chatHistoryList)
                {
                    chatHistory.isDeleted = true;
                    dbContext.Entry(chatHistory).State = EntityState.Modified;

                }
                await dbContext.SaveChangesAsync();
                return Json('1');
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Json('0');
            }
        }


        public static List<string> ExtractSqlQueries(string inputText)
        {
            var matches = Regex.Matches(inputText, @"(?:SELECT|INSERT).+?;", RegexOptions.Singleline);
            if (matches.Count == 0)
            {
                matches = Regex.Matches(inputText, @"```(.+?)```", RegexOptions.Singleline);
            }
            var queries = new List<string>();
            foreach (Match match in matches)
            {
                queries.Add(match.Value.Trim());
            }

            return queries;
        }
        public async Task<IActionResult> Resource(int Id)        {
                        
            switch (Id)
            {
                case 1:
                    return View("Views/TestingLLM/Viewfile1.cshtml");
                    
                case 2:
                    return View("Views/TestingLLM/Viewfile2.cshtml");
                   
                case 3:
                    return View("Views/TestingLLM/Viewfile3.cshtml");
                case 4:
                    return View("Views/TestingLLM/Viewfile4.cshtml");

                case 5:
                    return View("Views/TestingLLM/Viewfile5.cshtml");
                case 6:
                    return View("Views/TestingLLM/Viewfile6.cshtml");

                case 7:
                    return View("Views/TestingLLM/Viewfile7.cshtml");
                default:
                    return View("Views/TestingLLM/Viewfile1.cshtml");
                  
            }

        }
        public async Task<List<string>> GetSuggestions(string userInput)
        {
            try
            {
                var userId = string.Empty;
                var user = await _userManager.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefaultAsync();

                if (user != null)
                {
                    userId = user.Id;
                }
                if (string.IsNullOrEmpty(userId))
                {
                    return new List<string>();
                }
                               

                   var suggestions = dbContext.alloc8ChatGptHistory
                  .Where(prompt => prompt.userPrompt.StartsWith(userInput))
                  .Select(prompt => prompt.userPrompt)
                  .Distinct()
                  .Take(5)
                  .ToList();



                return suggestions;

            }
            catch (Exception e)
            {
                return new List<string>();
            }


        }
        [HttpPost]
        public async Task<IActionResult> Report(int id)
        {
            try
            {
                var data = dbContext.alloc8ChatGptHistory.FirstOrDefault(x => x.id == id);

                data.isReported = true;
                dbContext.alloc8ChatGptHistory.Update(data);
                dbContext.SaveChanges();

                return Json(new { status = 200, id = data.chatSessionId });
            }
            catch (Exception e)
            {
                return null;
            }


        }
        public async Task<IActionResult> AhFencingOpenApi(string prompt, string chatId)
        {
            var userId = string.Empty;
            try
            {
                var user = await _userManager.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefaultAsync();
                if (user != null)
                {
                    userId = user.Id;
                }
                if (string.IsNullOrEmpty(userId))
                {
                    return View();
                }

                bool hasData = false;
                var data = "";
                int queryId = 0;
                string openaiApiKey = "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";
                string apiUrl = "https://api.openai.com/v1/chat/completions";
                var promptExist = dbContext.alloc8ChatGptHistory.FirstOrDefault(x => x.userPrompt == prompt);
                if (promptExist == null || promptExist.url == null)
                {

                    string schema = " Provide proper sql queries with only provided columns and table names. CREATE TABLE xeroItemSaleDetails (\r\nid int NOT NULL,\r\nunitPrice decimal NULL,\r\ntaxType nvarchar(-1) NULL,\r\nitemid int NUL\r\n);\r\n\r\nCREATE TABLE simproAllPrebuilds (\r\nid int NOT NULL,\r\nprebuildsId int NOT NULL,\r\nhrefPrebuilds nvarchar(-1) NULL,\r\npartNo nvarchar(-1) NULL,\r\nname nvarchar(-1) NULL,\r\ndisplayOrder int NOT NULL,\r\narchived bit NOT NUL\r\n);\r\n\r\nCREATE TABLE xeroLineItems (\r\nid int NOT NULL,\r\nlineItemId nvarchar(-1) NULL,\r\nquoteid int NULL,\r\ndescription nvarchar(-1) NULL,\r\nunitAmount decimal NULL,\r\ndiscountAmount decimal NULL,\r\nlineAmount decimal NULL,\r\nitemCode nvarchar(-1) NULL,\r\nquantity decimal NULL,\r\ntaxAmount decimal NUL\r\n);\r\n\r\nCREATE TABLE simproAllProjectStatusCodes (\r\nid int NOT NULL,\r\nprojectStatusCodesId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\n\r\nCREATE TABLE xeroLinkedTransactions (\r\nid int NOT NULL,\r\nlinkedTransactionId nvarchar(-1) NULL,\r\nsourceTransactionId nvarchar(-1) NULL,\r\nsourceLineItemId nvarchar(-1) NULL,\r\ncontactid int NULL,\r\nstatus nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\nupdatedDateUtc datetime2 NULL,\r\nsourceTransactionTypeCode nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproAllProjectTags (\r\nid int NOT NULL,\r\nprojectTagsID int NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroManualJournals (\r\nid int NOT NULL,\r\nManualJournals nvarchar(-1) NULL,\r\nStatus nvarchar(-1) NULL,\r\nLineAmountTypes nvarchar(-1) NULL,\r\nUpdatedDateUTC datetime2 NULL,\r\nManualJournalID nvarchar(-1) NULL,\r\nNarration nvarchar(-1) NULL,\r\nHasAttachments bit NUL\r\n);\r\nCREATE TABLE simproAllQuoteArchiveReasons (\r\nid int NOT NULL,\r\narchiveReasonsId int NOT NULL,\r\narchiveReason nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroPayments (\r\nid int NOT NULL,\r\npaymentId nvarchar(-1) NULL,\r\ndate datetime2 NULL,\r\nbankAmount decimal NULL,\r\namount decimal NULL,\r\nreference nvarchar(-1) NULL,\r\ncurrencyRate decimal NULL,\r\npaymentType nvarchar(-1) NULL,\r\nstatus nvarchar(-1) NULL,\r\nupdatedDateUtc datetime2 NULL,\r\nhasAccount bit NULL,\r\nisReconciled bit NULL,\r\nbankAccountid int NULL,\r\ninvoiceid int NULL,\r\ncontactid int NULL,\r\nhasValidationErrors bit NULL,\r\nexpenseClaimid int NUL\r\n);\r\nCREATE TABLE simproAllQuoteAttachments (\r\nid int NOT NULL,\r\nquoteAttachmentsId nvarchar(-1) NULL,\r\nfilename nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroPurchaseOrders (\r\nid int NOT NULL,\r\npurchaseOrderId nvarchar(-1) NULL,\r\npurchaseOrderNumber nvarchar(-1) NULL,\r\ndateString datetime2 NULL,\r\ndate datetime2 NULL,\r\ndeliveryDateString nvarchar(-1) NULL,\r\ndeliveryDate datetime2 NULL,\r\ndeliveryAddress nvarchar(-1) NULL,\r\nattentionTo nvarchar(-1) NULL,\r\ntelephone nvarchar(-1) NULL,\r\ndeliveryInstructions nvarchar(-1) NULL,\r\nhasErrors bit NULL,\r\nisDiscounted bit NULL,\r\nreference nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\ncurrencyRate nvarchar(-1) NULL,\r\ncurrencyCode nvarchar(-1) NULL,\r\ncontactid int NULL,\r\nbrandingThemeid int NULL,\r\nstatus nvarchar(-1) NULL,\r\nlineAmountTypes nvarchar(-1) NULL,\r\nsubTotal decimal NULL,\r\ntotalTax decimal NULL,\r\ntotal decimal NULL,\r\nupdatedDateUtc datetime2 NULL,\r\nhasAttachments bit NUL\r\n);\r\nCREATE TABLE simproAllQuoteCostCenterCatalogItems (\r\nid int NOT NULL,\r\nquoteCostCenterCatalogId int NOT NULL,\r\nbasePrice real NOT NULL,\r\nmarkup real NOT NUL\r\n);\r\nCREATE TABLE xeroQuotes (\r\nid int NOT NULL,\r\nquoteId nvarchar(-1) NULL,\r\nquoteNumber nvarchar(-1) NULL,\r\ncontactid int NUL\r\n);\r\nCREATE TABLE simproAllQuoteCostCenterlaborItem (\r\nid int NOT NULL,\r\nlaborId int NOT NUL\r\n);\r\nCREATE TABLE xeroReceipts (\r\nid int NOT NULL,\r\nReceiptId nvarchar(-1) NULL,\r\nReceiptNumber nvarchar(-1) NULL,\r\nStatus nvarchar(-1) NULL,\r\nUserid int NULL,\r\nContactid int NULL,\r\nDate datetime2 NULL,\r\nUpdatedDateUTC datetime2 NULL,\r\nReference nvarchar(-1) NULL,\r\nLineAmountTypes nvarchar(-1) NULL,\r\nSubTotal decimal NULL,\r\nTotalTax decimal NULL,\r\nTotal decimal NULL,\r\nExpenseClaimid int NULL,\r\nHasAttachments bit NUL\r\n);\r\nCREATE TABLE simproAllQuoteCostCenterOneOff (\r\nid int NOT NULL,\r\nquotesOneOffId int NOT NULL,\r\ntype nvarchar(-1) NULL,\r\ndescription nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroTrackings (\r\nid int NOT NULL,\r\nTrackingOptionID nvarchar(-1) NULL,\r\nName nvarchar(-1) NULL,\r\nStatus nvarchar(-1) NULL,\r\nHasValidationErrors bit NULL,\r\nIsDeleted bit NULL,\r\nIsArchived bit NULL,\r\nIsActive bit NUL\r\n);\r\nCREATE TABLE simproAllQuoteCostCenters (\r\nid int NOT NULL,\r\nquoteCostCentersId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\ndateModified nvarchar(-1) NULL,\r\nhrefQuoteCostCenter nvarchar(-1) NULL,\r\ncostCenterId int NOT NULL,\r\ncostCenterName nvarchar(-1) NULL,\r\nquoteId int NOT NULL,\r\nquoteType nvarchar(-1) NULL,\r\nquoteName nvarchar(-1) NULL,\r\nquoteStage nvarchar(-1) NULL,\r\nquoteStatus nvarchar(-1) NULL,\r\nsectionId int NOT NULL,\r\nsectionName nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroUsers (\r\nid int NOT NULL,\r\nUserId nvarchar(-1) NULL,\r\nEmailAddress nvarchar(-1) NULL,\r\nFirstName nvarchar(-1) NULL,\r\nLastName nvarchar(-1) NULL,\r\nUpdatedDateUTC datetime2 NULL,\r\nIsSubscriber bit NULL,\r\nOrganisationRole nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproAllQuotes (\r\nid int NOT NULL,\r\nquoteId int NOT NULL,\r\ndescription nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproAllScheduleRates (\r\nid int NOT NULL,\r\nscheduleRatesId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproAllSchedules (\r\nid int NOT NULL,\r\ngetAllScheduleID int NULL,\r\ntype nvarchar(-1) NULL,\r\nreference nvarchar(-1) NULL,\r\ntotalHours real NULL,\r\ndate nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproAllSite (\r\nid int NOT NULL,\r\nsiteId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproAllStockItems (\r\nid int NOT NULL,\r\ncatalogId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\ninventoryCount int NOT NULL,\r\nstorageLocation nvarchar(-1) NULL,\r\ninventoryValue real NOT NULL,\r\npartNo nvarchar(-1) NULL,\r\ntradePrice real NOT NULL,\r\ntradePriceEx real NOT NULL,\r\ntradePriceInc real NOT NULL,\r\nsplitPrice int NOT NULL,\r\nsplitPriceEx int NOT NULL,\r\nsplitPriceInc int NOT NUL\r\n);\r\nCREATE TABLE simproAllStorageDevices (\r\nid int NOT NULL,\r\nstorageDevicesId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproAllTaxCodes (\r\nid int NOT NULL,\r\ntaxCodeId int NULL,\r\nhrefTaxCode nvarchar(-1) NULL,\r\ncode nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\nrate int NUL\r\n);\r\nCREATE TABLE simproAllUnitsOfMeasurement (\r\nid int NOT NULL,\r\nmeasurementId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproAssignedcostcenter (\r\nid int NOT NULL,\r\nassignedcostcenterId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE SimproAvilability (\r\nid int NOT NULL,\r\nstartDate nvarchar(-1) NULL,\r\nstartTime nvarchar(-1) NULL,\r\nendDate nvarchar(-1) NULL,\r\nendTime nvarchar(-1) NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE SimproBanking (\r\nid int NOT NULL,\r\naccountName nvarchar(-1) NULL,\r\nroutingNo nvarchar(-1) NULL,\r\naccountNo nvarchar(-1) NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE SimproCustomfield (\r\nid int NOT NULL,\r\nvalue nvarchar(-1) NULL,\r\nSimproEmployeeDataid int NUL\r\n);\r\nCREATE TABLE SimproCustomfield1 (\r\nid int NOT NULL,\r\ncustomfield1Id int NOT NULL,\r\nname nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\nisMandatory bit NOT NULL,\r\nlistItems nvarchar(-1) NULL,\r\ncustomfieldid int NUL\r\n);\r\nCREATE TABLE SimproDefaultcompany (\r\nid int NOT NULL,\r\ndefaultcompanyId int NULL,\r\nname nvarchar(-1) NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE SimproEmergencycontact (\r\nid int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nrelationship nvarchar(-1) NULL,\r\nworkPhone nvarchar(-1) NULL,\r\ncellPhone nvarchar(-1) NULL,\r\naltPhone nvarchar(-1) NULL,\r\naddress nvarchar(-1) NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE simproEmployeeData (\r\nid int NOT NULL,\r\nemployeeDataID int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nposition nvarchar(-1) NULL,\r\ndateOfHire nvarchar(-1) NULL,\r\ndateOfBirth nvarchar(-1) NULL,\r\ndateCreated datetime2 NULL,\r\ndateModified datetime2 NULL,\r\narchived bit NOT NULL,\r\nzones nvarchar(-1) NULL,\r\nmaskedSSN nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproPayrates (\r\nid int NOT NULL,\r\npayRate int NULL,\r\nemploymentCost nvarchar(-1) NULL,\r\noverhead int NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE SimproPrimarycontact (\r\nid int NOT NULL,\r\nemail nvarchar(-1) NULL,\r\nsecondaryEmail nvarchar(-1) NULL,\r\nworkPhone nvarchar(-1) NULL,\r\nextension nvarchar(-1) NULL,\r\ncellPhone nvarchar(-1) NULL,\r\nfax nvarchar(-1) NULL,\r\npreferredNotificationMethod nvarchar(-1) NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE SimproQuoteAmountCostCenter (\r\nid int NOT NULL,\r\nexTax real NOT NULL,\r\nincTax real NOT NULL,\r\nsimproQuoteTotalCostCenterid int NUL\r\n);\r\nCREATE TABLE SimproQuoteAmountCostCenterlaborItem (\r\nid int NOT NULL,\r\nexTax real NOT NULL,\r\nincTax real NOT NULL,\r\nquoteTotalCostCenterlaborid int NUL\r\n);\r\nCREATE TABLE SimproQuoteAmountCostCenterOneOff (\r\nid int NOT NULL,\r\nexTax nvarchar(-1) NULL,\r\nincTax nvarchar(-1) NULL,\r\nsimproQuoteTotalCostCenterOneOffid int NUL\r\n);\r\nCREATE TABLE SimproQuoteCatalogCostCenter (\r\nid int NOT NULL,\r\ncatalogCostCenterID int NOT NULL,\r\npartNo nvarchar(-1) NULL,\r\nname nvarchar(-1) NULL,\r\nallQuoteCostCenterCatalogItemsid int NUL\r\n);\r\nCREATE TABLE SimproQuoteLabortypeCostCenter (\r\nid int NOT NULL,\r\nlabortypeId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nquoteCostCenterlaborItemsid int NUL\r\n);\r\nCREATE TABLE SimproQuoteSellpriceCostCenter (\r\nid int NOT NULL,\r\nexTax real NOT NULL,\r\nincTax real NOT NULL,\r\nallQuoteCostCenterCatalogItemsid int NUL\r\n);\r\nCREATE TABLE SimproQuoteSellpriceCostCenterlaborItem (\r\nid int NOT NULL,\r\nexTax real NOT NULL,\r\nincTax real NOT NULL,\r\nquoteCostCenterlaborItemsid int NUL\r\n);\r\nCREATE TABLE SimproQuoteSellpriceCostCenterOneOff (\r\nid int NOT NULL,\r\nexTax nvarchar(-1) NULL,\r\nincTax nvarchar(-1) NULL,\r\nsimproAllQuoteCostCenterOneOffid int NUL\r\n);\r\nCREATE TABLE SimproQuoteTotalCostCenter (\r\nid int NOT NULL,\r\nqty real NOT NULL,\r\nallQuoteCostCenterCatalogItemsid int NUL\r\n);\r\nCREATE TABLE SimproQuoteTotalCostCenterlabor (\r\nid int NOT NULL,\r\nqty int NOT NULL,\r\nquoteCostCenterlaborItemsid int NUL\r\n);\r\nCREATE TABLE SimproQuoteTotalCostCenterOneOff (\r\nid int NOT NULL,\r\nqty int NOT NULL,\r\nsimproAllQuoteCostCenterOneOffid int NUL\r\n);\r\nCREATE TABLE __EFMigrationsHistory (\r\nMigrationId nvarchar(150) NOT NULL,\r\nProductVersion nvarchar(32) NOT NUL\r\n);\r\nCREATE TABLE SimproReference (\r\nid int NOT NULL,\r\ntype nvarchar(-1) NULL,\r\nnumber nvarchar(-1) NULL,\r\ntext nvarchar(-1) NULL,\r\njobNotesid int NUL\r\n);\r\nCREATE TABLE configration (\r\nid int NOT NULL,\r\nxeroClientId nvarchar(-1) NULL,\r\nxeroClientSecret nvarchar(-1) NULL,\r\nxeroCallBackUrl nvarchar(-1) NULL,\r\nxeroScopes nvarchar(-1) NULL,\r\nxeroTenantId nvarchar(-1) NULL,\r\nxeroRefreshToken nvarchar(-1) NULL,\r\nxeroRefreshTokenExpiresAt datetime2 NULL,\r\nxeroAccessToken nvarchar(-1) NULL,\r\nxeroAccessTokenExpiresAt datetime2 NULL,\r\nupdateAt datetime2 NOT NULL,\r\ngoogleAccessToken nvarchar(-1) NULL,\r\ngoogleAccessTokenExpiresAt datetime2 NULL,\r\ngoogleCallBackUrl nvarchar(-1) NULL,\r\ngoogleClientId nvarchar(-1) NULL,\r\ngoogleClientSecret nvarchar(-1) NULL,\r\ngoogleRefreshToken nvarchar(-1) NULL,\r\ngoogleRefreshTokenExpiresAt datetime2 NULL,\r\ngoogleScopes nvarchar(-1) NULL,\r\ngoogleTenantId nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproReportCostToCompleteFinancialView (\r\nid int NOT NULL,\r\nrequestNumber nvarchar(-1) NULL,\r\ntotal real NULL,\r\nclaimedToDate real NULL,\r\nbilledPercentage real NULL,\r\ncostToDate real NULL,\r\ncostToComplete real NULL,\r\npercentageComplete real NULL,\r\nnetMarginToDate real NULL,\r\nprojectedNetMargin real NULL,\r\nfinancialViewsJobId int NULL,\r\nfinancialViewsCustomerId int NULL,\r\nfinancialViewsSiteID int NUL\r\n);\r\nCREATE TABLE gDriveDocumentLogs (\r\nid int NOT NULL,\r\nname nvarchar(-1) NULL,\r\ndocumentId nvarchar(-1) NULL,\r\npath nvarchar(-1) NULL,\r\nextension nvarchar(-1) NULL,\r\nisDownloaded bit NOT NULL,\r\nisDeleted bit NOT NULL,\r\ncreatedAt datetime2 NOT NULL,\r\nupdateAt datetime2 NOT NUL\r\n);\r\nCREATE TABLE simproReportCostToCompleteOperationsView (\r\nid int NOT NULL,\r\nrequestNumber nvarchar(-1) NULL,\r\njobID int NULL,\r\ncustomerID int NULL,\r\nsiteId int NULL,\r\noriginalestimatedbudgetMaterials real NULL,\r\noriginalestimatedbudgetResources real NULL,\r\noriginalestimatedbudgetResourceHours real NULL,\r\nrevisedestimatedbudgetMaterials int NULL,\r\nrevisedestimatedbudgetResources int NULL,\r\nrevisedestimatedbudgetResourceHours int NULL,\r\nrevizedestimatedbudgetMaterials int NULL,\r\nrevizedestimatedbudgetResources int NULL,\r\nrevizedestimatedbudgetResourceHours int NULL,\r\ncurrentbudgetMaterials real NULL,\r\ncurrentbudgetResources real NULL,\r\ncurrentbudgetResourceHours real NULL,\r\nactualtodateMaterials real NULL,\r\nactualtodateResources real NULL,\r\nactualtodateResourceHours real NULL,\r\nforecastremainingMaterials real NULL,\r\nforecastremainingResources real NULL,\r\nforecastremainingResourceHours real NULL,\r\nvarianceMaterials real NULL,\r\nvarianceResources real NULL,\r\nvarianceResourceHours real NULL,\r\npercentageMaterials real NULL,\r\npercentageResources real NULL,\r\npercentageResourceHours real NUL\r\n);\r\nCREATE TABLE SimproAccountsetup (\r\nid int NOT NULL,\r\nusername nvarchar(-1) NULL,\r\nisMobility bit NOT NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE simproRetrieveDetailsForASpecificLaborRateOverhead (\r\nid int NOT NULL,\r\noverhead int NOT NUL\r\n);\r\nCREATE TABLE SimproAddressData (\r\nid int NOT NULL,\r\naddress nvarchar(-1) NULL,\r\ncity nvarchar(-1) NULL,\r\nstate nvarchar(-1) NULL,\r\npostalCode nvarchar(-1) NULL,\r\ncountry nvarchar(-1) NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE SimproScheduleBlock (\r\nid int NOT NULL,\r\nhrs real NULL,\r\nstartTime nvarchar(-1) NULL,\r\niso8601StartTime datetime2 NULL,\r\nendTime nvarchar(-1) NULL,\r\niso8601EndTime datetime2 NULL,\r\ngetAllScheduleid int NUL\r\n);\r\nCREATE TABLE simproAllAccountCostCenters (\r\nId int NOT NULL,\r\naccountCostCentersId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproSchedulerate (\r\nid int NOT NULL,\r\nsimproSchedulerateID int NULL,\r\nname nvarchar(-1) NULL,\r\nsimproScheduleBlockid int NUL\r\n);\r\nCREATE TABLE simproAllActivitySchedules (\r\nid int NOT NULL,\r\nsimproActivityScheduleaID int NULL,\r\ntotalHours real NULL,\r\nstaffID int NULL,\r\nstaffName nvarchar(-1) NULL,\r\nstaffType nvarchar(-1) NULL,\r\nstaffTypeId int NOT NULL,\r\ndate nvarchar(-1) NULL,\r\nactivityID int NULL,\r\nactivityName nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproScheduleStaff (\r\nid int NOT NULL,\r\nscheduleStaffID int NULL,\r\nname nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\ntypeId int NULL,\r\ngetAllScheduleid int NUL\r\n);\r\nCREATE TABLE simproAllCatalogInventories (\r\nid int NOT NULL,\r\ninventoriesId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nstorageLocation nvarchar(-1) NULL,\r\nminimumLevel int NOT NULL,\r\nrestockLevel int NOT NUL\r\n);\r\nCREATE TABLE SimproSectionsClaimed (\r\nid int NOT NULL,\r\ntodatePercent int NULL,\r\namountExTax int NULL,\r\namountIncTax int NULL,\r\nremainingPercent int NULL,\r\namount1ExTax real NULL,\r\namount1IncTax real NULL,\r\nSimproAllJobsSectionsCostCenterid int NUL\r\n);\r\nCREATE TABLE simproAllCatalogItem (\r\nid int NOT NULL,\r\ncatalogItemId int NOT NULL,\r\npartNo nvarchar(-1) NULL,\r\nname nvarchar(-1) NULL,\r\ntradePrice real NOT NULL,\r\ntradePriceEx real NOT NULL,\r\ntradePriceInc real NOT NULL,\r\nsplitPrice int NOT NULL,\r\nsplitPriceEx int NOT NULL,\r\nsplitPriceInc int NOT NUL\r\n);\r\nCREATE TABLE SimproSectionsCostcenter (\r\nid int NOT NULL,\r\nsimproSectionsCostcenterId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nallJobsSectionsCostCenterid int NUL\r\n);\r\nCREATE TABLE simproAllCatalogItemVendors (\r\nid int NOT NULL,\r\nvendorId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nvendorPartNo nvarchar(-1) NULL,\r\ndefaultVender bit NOT NUL\r\n);\r\nCREATE TABLE SimproSectionsTaxcode (\r\nid int NOT NULL,\r\nsimproSectionsTaxcodeId int NOT NULL,\r\ncode nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\nrate int NOT NULL,\r\nallJobsSectionsCostCenterid int NULL,\r\nSimproSectionsTotalid int NUL\r\n);\r\nCREATE TABLE simproAllChartOfAccounts (\r\nid int NOT NULL,\r\naccountsId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproSectionsTotal (\r\nid int NOT NULL,\r\nexTax real NOT NULL,\r\ntax real NOT NULL,\r\nincTax real NOT NULL,\r\nallJobsSectionsCostCenterid int NUL\r\n);\r\nCREATE TABLE simproAllContacts (\r\nid int NOT NULL,\r\ncontactsId int NULL,\r\ngivenName nvarchar(-1) NULL,\r\nfamilyName nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproSecurityGroup (\r\nid int NOT NULL,\r\nsecuritygroupId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\naccountsetupid int NUL\r\n);\r\nCREATE TABLE simproAllCustomerPayments (\r\nid int NOT NULL,\r\ncustomerPaymentsId int NOT NULL,\r\npaymentDepositAccount nvarchar(-1) NULL,\r\npaymentDate nvarchar(-1) NULL,\r\npaymentFinanceCharge int NULL,\r\npaymentCheckNo nvarchar(-1) NULL,\r\npaymentDetails nvarchar(-1) NULL,\r\npaymentFinanceFeeBreakdown nvarchar(-1) NULL,\r\npaymentmethodId int NOT NULL,\r\npaymentMethodName nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproTotal (\r\nid int NOT NULL,\r\nexTax real NOT NULL,\r\ntax real NOT NULL,\r\nincTax real NOT NULL,\r\njobid int NUL\r\n);\r\nCREATE TABLE simproAllCustomers (\r\nid int NOT NULL,\r\ncustomersId int NULL,\r\nhrefCustomers nvarchar(-1) NULL,\r\ncompanyName nvarchar(-1) NULL,\r\ngivenName nvarchar(-1) NULL,\r\nfamilyName nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproTotalQuote (\r\nid int NOT NULL,\r\nexTax real NOT NULL,\r\ntax real NOT NULL,\r\nincTax real NOT NULL,\r\nsimproAllQuotesid int NUL\r\n);\r\nCREATE TABLE simproAllEmployeeLicences (\r\nid int NOT NULL,\r\nemployeeLicenceId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nrefData nvarchar(-1) NULL,\r\nExpiryDate nvarchar(-1) NUL\r\n);\r\nCREATE TABLE SimproUserprofile (\r\nid int NOT NULL,\r\nisSalesperson bit NOT NULL,\r\nisProjectManager bit NOT NULL,\r\npreferredLanguage nvarchar(-1) NULL,\r\nemployeeDataid int NUL\r\n);\r\nCREATE TABLE simproAllEmployees (\r\nid int NOT NULL,\r\nemployeeId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroAllocations (\r\nid int NOT NULL,\r\nallocationId nvarchar(-1) NULL,\r\ndate datetime2 NULL,\r\namount decimal NULL,\r\ncreditNoteid int NULL,\r\ninvoiceid int NUL\r\n);\r\nCREATE TABLE simproAllInvoices (\r\nid int NOT NULL,\r\ninvoicesId int NOT NULL,\r\ntype nvarchar(-1) NULL,\r\nrecurringInvoice nvarchar(-1) NULL,\r\nisPaid bit NOT NULL,\r\ncustomerId int NOT NULL,\r\ncustomerCompanyName nvarchar(-1) NULL,\r\ncustomerGivenName nvarchar(-1) NULL,\r\ncustomerFamilyName nvarchar(-1) NULL,\r\ntotalExTax real NOT NULL,\r\ntotalIncTax real NOT NULL,\r\ntotalTax real NOT NULL,\r\ntotalReverseChargeTax int NOT NULL,\r\ntotalAmountApplied nvarchar(-1) NULL,\r\ntotalBalanceDue real NOT NULL,\r\njobInvoiceId int NOT NULL,\r\njobDescription nvarchar(-1) NULL,\r\ntotal1ExTax real NOT NULL,\r\ntotal1Tax real NOT NULL,\r\ntotal1IncTax real NOT NUL\r\n);\r\nCREATE TABLE xeroBankAccounts (\r\nid int NOT NULL,\r\naccountId nvarchar(-1) NULL,\r\ncode nvarchar(-1) NULL,\r\nname nvarchar(-1) NULL,\r\nstatus nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\ntaxType nvarchar(-1) NULL,\r\nenablePaymentsToAccount bit NULL,\r\nshowInExpenseClaims bit NULL,\r\nbankAccountNumber nvarchar(-1) NULL,\r\nbankAccountType nvarchar(-1) NULL,\r\ncurrencyCode nvarchar(-1) NULL,\r\nreportingCode nvarchar(-1) NULL,\r\nreportingCodeName nvarchar(-1) NULL,\r\nhasAttachments bit NULL,\r\nupdatedDateUTC datetime2 NULL,\r\naddToWatchlist bit NUL\r\n);\r\nCREATE TABLE simproAllInvoicRetainages (\r\nid int NOT NULL,\r\nretainagesId int NOT NULL,\r\nisPaid bit NOT NULL,\r\ncustomerId int NOT NULL,\r\ncustomerCompanyName nvarchar(-1) NULL,\r\ncustomerGivenName nvarchar(-1) NULL,\r\ncustomerFamilyName nvarchar(-1) NULL,\r\njobId int NOT NULL,\r\njobDescription nvarchar(-1) NULL,\r\ntotalExTax real NULL,\r\ntotalTax real NULL,\r\ntotalIncTax real NULL,\r\ntotal1ExTax real NOT NULL,\r\ntotal1IncTax real NOT NULL,\r\ntotal1Tax real NOT NULL,\r\ntotal1ReverseChargeTax int NOT NULL,\r\ntotal1AmountApplied real NOT NULL,\r\ntotal1BalanceDue real NOT NUL\r\n);\r\nCREATE TABLE xeroBankTransactions (\r\nid int NOT NULL,\r\nbankTransactionId nvarchar(-1) NULL,\r\nbankAccountid int NULL,\r\ntype nvarchar(-1) NULL,\r\nreference nvarchar(-1) NULL,\r\nisReconciled bit NULL,\r\nhasAttachments bit NULL,\r\ncontactid int NULL,\r\ndateString datetime2 NULL,\r\ndate datetime2 NULL,\r\nstatus nvarchar(-1) NULL,\r\nlineAmountTypes nvarchar(-1) NULL,\r\nsubTotal decimal NULL,\r\ntotalTax decimal NULL,\r\ntotal decimal NULL,\r\nupdatedDateUTC datetime2 NULL,\r\ncurrencyCode nvarchar(-1) NUL\r\n);\r\nCREATE TABLE simproAllJob (\r\nid int NOT NULL,\r\njobId int NOT NULL,\r\ndescription nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroBankTransfers (\r\nid int NOT NULL,\r\nbankTransferId nvarchar(-1) NULL,\r\ncreatedDateUtcString nvarchar(-1) NULL,\r\ncreatedDateUtc datetime2 NULL,\r\ndateString nvarchar(-1) NULL,\r\ndate datetime2 NULL,\r\namount decimal NULL,\r\nfromBankAccountid int NULL,\r\ntoBankAccountid int NULL,\r\nfromBankTransactionid int NULL,\r\ntoBankTransactionid int NULL,\r\nfromIsReconciled bit NULL,\r\ncurrencyRate decimal NULL,\r\nreference nvarchar(-1) NULL,\r\nhasAttachments bit NUL\r\n);\r\nCREATE TABLE simproAllJobAttachment (\r\nid int NOT NULL,\r\njobAttachmentId nvarchar(-1) NULL,\r\nfilename nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroBrandingThemes (\r\nid int NOT NULL,\r\nbrandingThemeId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nlogoUrl nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\nsortOrder nvarchar(-1) NULL,\r\ncreatedDateUtc datetime2 NUL\r\n);\r\nCREATE TABLE simproAllJobAttachmentFolder (\r\nid int NOT NULL,\r\njobAttachmentFolderId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroContactAddresses (\r\nid int NOT NULL,\r\naddressType nvarchar(-1) NULL,\r\naddressLine1 nvarchar(-1) NULL,\r\naddressLine2 nvarchar(-1) NULL,\r\naddressLine3 nvarchar(-1) NULL,\r\ncity nvarchar(-1) NULL,\r\nregion nvarchar(-1) NULL,\r\npostalCode nvarchar(-1) NULL,\r\ncountry nvarchar(-1) NULL,\r\ncontactid int NUL\r\n);\r\nCREATE TABLE simproAllJobCostCenter (\r\nid int NOT NULL,\r\njobCostCenterId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\ndateModified nvarchar(-1) NULL,\r\nhref nvarchar(-1) NULL,\r\ncostcenterid int NOT NULL,\r\ncostcenterName nvarchar(-1) NULL,\r\njobId int NOT NULL,\r\njobType nvarchar(-1) NULL,\r\njobName nvarchar(-1) NULL,\r\njobStage nvarchar(-1) NULL,\r\njobStatus nvarchar(-1) NULL,\r\nsectionId int NOT NULL,\r\nsectionName nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroContactGroups (\r\nid int NOT NULL,\r\ncontactGroupId nvarchar(-1) NULL,\r\nname nvarchar(-1) NULL,\r\nstatus nvarchar(-1) NULL,\r\nhasValidationErrors bit NUL\r\n);\r\nCREATE TABLE simproAllJobCostCenterCatalogItems (\r\nid int NOT NULL,\r\ncostCenterCatalogId int NOT NULL,\r\nbasePrice real NULL,\r\nmarkup nvarchar(-1) NULL,\r\nclaimed nvarchar(-1) NULL,\r\ncatalogId int NOT NULL,\r\ncatalogPartNo nvarchar(-1) NULL,\r\ncatalogName nvarchar(-1) NULL,\r\nsellPriceExTax real NULL,\r\nsellPriceIncTax real NULL,\r\ntotalQty real NULL,\r\namountExTax real NULL,\r\namountIncTax real NUL\r\n);\r\nCREATE TABLE xeroContactPhones (\r\nid int NOT NULL,\r\nphoneType nvarchar(-1) NULL,\r\nphoneNumber nvarchar(-1) NULL,\r\nphoneAreaCode nvarchar(-1) NULL,\r\nphoneCountryCode nvarchar(-1) NULL,\r\ncontactid int NUL\r\n);\r\nCREATE TABLE simproAllJobCostCenterLaborItems (\r\nid int NOT NULL,\r\nlaborId int NOT NULL,\r\nclaimed nvarchar(-1) NULL,\r\ntypeId int NULL,\r\ntypeName nvarchar(-1) NULL,\r\nsellPriceExTax real NULL,\r\nsellPriceIncTax real NULL,\r\ntotalQty int NULL,\r\namountExTax real NULL,\r\namountIncTax real NUL\r\n);\r\nCREATE TABLE xeroContacts (\r\nid int NOT NULL,\r\ncontactId nvarchar(-1) NULL,\r\naccountNumber nvarchar(-1) NULL,\r\ncontactStatus nvarchar(-1) NULL,\r\nname nvarchar(-1) NULL,\r\nfirstName nvarchar(-1) NULL,\r\nlastName nvarchar(-1) NULL,\r\nemailAddress nvarchar(-1) NULL,\r\nupdatedDateUtc datetime2 NULL,\r\nisSupplier bit NULL,\r\nisCustomer bit NULL,\r\nhasAttachments bit NULL,\r\nhasValidationErrors bit NUL\r\n);\r\nCREATE TABLE simproAllJobCostCenterOneOffItems (\r\nid int NOT NULL,\r\ncostCenterOneOffId int NOT NULL,\r\ntype nvarchar(-1) NULL,\r\ndescription nvarchar(-1) NULL,\r\nclaimed nvarchar(-1) NULL,\r\nsellPriceExTax nvarchar(-1) NULL,\r\nsellPriceIncTax real NULL,\r\ntotalQty int NULL,\r\namountExTax nvarchar(-1) NULL,\r\namountIncTax real NUL\r\n);\r\nCREATE TABLE xeroCreditNotes (\r\nid int NOT NULL,\r\ncreditNoteId nvarchar(-1) NULL,\r\ncreditNoteNumber nvarchar(-1) NULL,\r\nhasErrors bit NULL,\r\ntype nvarchar(-1) NULL,\r\nreference nvarchar(-1) NULL,\r\nremainingCredit decimal NULL,\r\nhasAttachments bit NULL,\r\ncontactid int NULL,\r\ndateString nvarchar(-1) NULL,\r\ndate datetime2 NULL,\r\ndueDateString nvarchar(-1) NULL,\r\ndueDate datetime2 NULL,\r\nstatus nvarchar(-1) NULL,\r\nlineAmountTypes nvarchar(-1) NULL,\r\nsubTotal decimal NULL,\r\ntotalTax decimal NULL,\r\ntotal decimal NULL,\r\nupdatedDateUtc datetime2 NULL,\r\ncurrencyCode nvarchar(-1) NULL,\r\nfullyPaidOnDate datetime2 NULL,\r\ninvoiceid int NUL\r\n);\r\nCREATE TABLE simproAllJobNote (\r\nid int NOT NULL,\r\njobNotesId int NOT NULL,\r\nsubject nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroExpenseClaims (\r\nid int NOT NULL,\r\nexpenseClaimId nvarchar(-1) NULL,\r\nstatus nvarchar(-1) NULL,\r\nupdatedDateUtc datetime2 NULL,\r\nuserid int NULL,\r\ntotal decimal NULL,\r\namountDue decimal NULL,\r\namountPaid decimal NULL,\r\npaymentDueDate datetime2 NULL,\r\nreportingDate datetime2 NUL\r\n);\r\nCREATE TABLE simproAllJobsSectionsCostCenters (\r\nid int NOT NULL,\r\nsimproAllJobsSectionsCostCenterId int NOT NULL,\r\njobId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\ndisplayOrder int NOT NULL,\r\npercentComplete nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroInvoices (\r\nid int NOT NULL,\r\ninvoiceId nvarchar(-1) NULL,\r\ntype nvarchar(-1) NULL,\r\ninvoiceNumber nvarchar(-1) NULL,\r\nreference nvarchar(-1) NULL,\r\namountDue decimal NULL,\r\namountPaid decimal NULL,\r\namountCredited decimal NULL,\r\ncurrencyRate decimal NULL,\r\nisDiscounted bit NULL,\r\nhasAttachments bit NULL,\r\nhasErrors bit NULL,\r\ncontactid int NULL,\r\ndateString datetime2 NULL,\r\ndate datetime2 NULL,\r\ndueDateString datetime2 NULL,\r\ndueDate datetime2 NULL,\r\nbrandingThemeId nvarchar(-1) NULL,\r\nstatus nvarchar(-1) NULL,\r\nlineAmountTypes nvarchar(-1) NULL,\r\nsubTotal decimal NULL,\r\ntotalTax decimal NULL,\r\ntotal decimal NULL,\r\nupdatedDateUtc datetime2 NULL,\r\ncurrencyCode decimal NUL\r\n);\r\nCREATE TABLE simproAllLaborRates (\r\nid int NOT NULL,\r\nlaborRatesId int NOT NULL,\r\nname nvarchar(-1) NUL\r\n);\r\nCREATE TABLE xeroItemPurchaseDetails (\r\nid int NOT NULL,\r\nunitPrice decimal NULL,\r\ntaxType nvarchar(-1) NULL,\r\nitemid int NUL\r\n);\r\nCREATE TABLE simproAllPrebuildCatalogs (\r\nid int NOT NULL,\r\nprebuildCatalogsId int NOT NULL,\r\nquantity int NOT NULL,\r\ndisplayOrder int NOT NULL,\r\npartNo nvarchar(-1) NULL,\r\nname nvarchar(-1) NULL,\r\ntradePrice real NOT NULL,\r\ntradePriceEx real NOT NULL,\r\ntradePriceInc real NOT NULL,\r\nsplitPrice int NOT NULL,\r\nsplitPriceEx int NOT NULL,\r\nsplitPriceInc int NOT NUL\r\n);\r\nCREATE TABLE xeroItems (\r\nid int NOT NULL,\r\nitemId nvarchar(-1) NULL,\r\ncode nvarchar(-1) NULL,\r\ndescription nvarchar(-1) NULL,\r\npurchaseDescription nvarchar(-1) NULL,\r\nname nvarchar(-1) NULL,\r\nisTrackedAsInventory bit NULL,\r\nisSold bit NULL,\r\nisPurchased bit NULL,\r\nupdatedDateUtc datetime2 NULL,\r\nsaleAccountCode nvarchar(-1) NULL,\r\nsaleCogsAccountCode nvarchar(-1) NULL,\r\nsaleTaxType nvarchar(-1) NULL,\r\nsaleUnitPrice decimal NULL,\r\npurchaseAccountCode nvarchar(-1) NULL,\r\npurchaseCogsAccountCode nvarchar(-1) NULL,\r\npurchaseTaxType nvarchar(-1) NULL,\r\npurchaseUnitPrice decimal NUL\r\n);\r\nCREATE TABLE simproAllPrebuildGroups (\r\nid int NOT NULL,\r\nprebuildGroupId int NOT NULL,\r\nname nvarchar(-1) NULL,\r\nparentGroup nvarchar(-1) NUL\r\n);\r\n\r\nDescriptions:\r\n\r\n1. The table simproAllContacts has one primary key id and having 3 other columns named contactId, givenName, FamilyName. This table contains the personal information of the user.\r\n\r\n2. The table simproAllCustomers has one primary key id and this table contains companyName with customerid.\r\n\r\n3. The table simproAllEmployees has one primary key id and this table contains employeeId and name of all the employees.\r\n\r\n4. The table simproAllSites has one primary key id and this table contains siteId and name of all the sites.\r\n\r\n5. The table simproEmployeeData has one primary key id and this table contains employeeDataId , name, position , dateOfHire of the employee. The SimproAddressData is linked with various tables as mentioning next. The table simproEmployeeData employeeDataid is a foreign key of this table. The table SimproAvilability id is primary key of this table and employeeDataid is foreign key. The SimproPrimarycontact id is primary key of this table and employeeDataid is foreign key. The SimproEmergencycontact id is primary key of this table and employeeDataid is foreign key. The table SimproAccountsetup id is primary key of this table and employeeDataid is foreign key. The table SimproUserprofile id is primary key of this table and employeeDataid is foreign key. the table Simprosecuritygroup id is primary key of this table and accountsetupid is foreign key. \r\nThe table SimproDefaultcompany id is primary key of this table and employeeDataid is foreign key.\r\nthe table SimproBanking id is primary key of this table and employeeDataid is foreign key.\r\nThe table SimproPayrates id is primary key of this table and employeeDataid is foreign key.\r\nThe table SimproAssignedcostcenter id is primary key of this table and employeeDataid is foreign key.\r\nThe table SimproCustomfield id is primary key of this table and employeeDataid is foreign key.\r\nThe table SimproCustomfield1 id is primary key of this table and customfieldid is foreign key.\r\n\r\n6. The table simproEmployeeLicences has one primary key id and this table contains employeeLicenseId, name , refData and ExpiryDate of all employees.\r\n\r\n7. The table simproAllJob has one primary key id and this table contains jobid, description and it is linked with one more table named SimproTotal id is primary key of this table and jobid is foreign key.\r\n\r\n8. The table simproAllJobNotes has one primary key id and this table contains notesid, jobnotesid, subject and it is linked with one more table named SimproReference id is primary key of this table and jobNotesid is foreign key.\r\n\r\n9. The table simproAllJobAttachment has one primary key id and this table contains jobAttachmentId , filename of all jobs.\r\n\r\n10. The table simproAllJobAttachmentFolder has one primary key id and this table contains jobattachmentfolderid and name of jobs.\r\n\r\n11. The table simproAllJobCostCenter has one primary key id and this table contains jobcostcenterid, name , dateModified and hrefcostcenterid, costcenterName, jobId, jobType, jobName, jobStage, jobStatus, sectionId, sectionName of All job Cost center.\r\n\r\n14. The table simproAllJobSectionsCostCenters has one primary key id and this table contains simproAllJobSectionsCostCentersid, jobId, name, displayOrder, percentComplete. It is linked with various more table like SimproSectionsCostcenter id is primary key of this table and allJobsSectionsCostCenterid is foreign key, SimproSectionsTotal id is primary key of this table and allJobsSectionCostCenterid is foreign key, SimproSectionsTaxcode id is primary key of this table and has two foreign two foreign keys allJobsSectionsCostCenterid and simproSectionsTotalid, SimproSectionsClaimed id is primary key of this table and simproallJobsSectionsCostCenterid is foreign key.\r\n\r\n15. The table simproAllJobCostCenterCatalogItems has one primary key id and this table contains costcentercatalogid , basePrice, markup, claimed, catalogId, catalogPartNo, catalogName, sellPriceExTax, sellPriceIncTax, totalQty, amountExtax, amountIncTax of all job cost center. \r\n\r\n16. The table simproAllJobCostCenterlaborItems has one primary key id and this table contains labourId, claimed, typeId, typeName, sellPriceExTax, sellPriceIncTax, totalQty, amountExtax, amountIncTax of all labor items.\r\n\r\n17. The table simproAllJobCostCenterOneOffItems has one primary key id and this table contains costCenterOneOffId, type, description, claimed,  sellPriceExTax, sellPriceIncTax, totalQty, amountExtax, amountIncTax of all one off items.\r\n\r\n18. The table simproAllQuotes has one primary key id and this table contains quoteId, description of quotes and one table linked with it named SimproTotalQuote id is primary key of this table and simproAllQuotesid is foreign key.\r\n\r\n19. The table simproAllQuoteAttachments has one primary key id and this table contains quotesAttachmentsId, filename of all quotes attachments.\r\n\r\n20. The table simproAllQuoteCostCenters has one primary key id and this table contains quoteCostCentersId, name, dateModfied, hrefQuoteCostCenter, costCenterId, costCenterName, quoteId, quotetype, quoteName, quotestage, quoteStatus, sectionId. sectionName of all quotes cost centers.\r\n\r\n21. The table simproAllQuoteCostCenterCatalogItems has one primary key id and this table contains quoteCostCenterCatalogId, basePrice, markUp. It is linked with various more table like SimproQuoteCatalogCostCenter id is primary key of this table and allQuoteCostCenterCatalogItemsid is foreign key , SimproQuoteSellPriceCostCenter id is primary key of this table and allQuoteCostCenterCatalogItemsid is foreign key, SimproQuoteTotalCostCenter id is primary key of this table and allQuoteCostCenterCatalogItemsid is foreign key, SimproQuoteAmountCostCenter id is primary key of this table and simproQuoteTotalCostCenterid is foreign key.\r\n\r\n22. The table simproAllQuoteCostCenterLaborItem has one primary key id and this table contains laborid. It is linked with various more table like SimproQuoteLabortypeCostCenter id is primary key of this table and quoteCostCenterLaborItemsid is foreign key , SimproQuoteSellPriceCostCenterLaborItem id is primary key of this table and quoteCostCenterLaborItemsid is foreign key, SimproQuoteTotalCostCenterLabor id is primary key of this table and quoteCostCenterLaborItemsid is foreign key, SimproQuoteAmountCostCenterLaborItem id is primary key of this table and quoteTotalCostCenterLaborid is foreign key.\r\n\r\n23. The table simproAllQuoteCostCenterOneOff has one primary key id and this table contains quotesOneOffId, typeDescription. It is linked with various more tables like SimproQuoteSellPriceCostCenterOneOff id is primary key of this table and simproQuoteSellPriceCostCenterOneOffid is foreign key, SimproQuoteTotalCostCenterOneOff id is primary key of this table and simproQuoteCostCenterOneOffid is foreign key, SimproQuoteAmountCostCenterOneOff id is primary key of this table and simproQuoteTotalCostCenterOneOffid is foreign key.\r\n\r\n24. The table simproAllCatalogItem has one primary key id and this table contains catalogItemId, partNo, name, tradePrice, tradePriceEx, tradePriceInc, splitPriceEx, splitPriceInc of all catalog items\r\n\r\n25. Table xeroManualJournals contains mannual journals of xero has some status for mannual journals .\r\n26. Table xeroTrackings contains tracking option id and had tracking status and has validtions errors .\r\n27. Table xeroBankTransfers contains amount of transfer and currency rate and it has some columns FromBankAccountId,ToBankAccountId,FromBankTransactionId,ToBankTransactionId which are foreign keys in another tables.\r\n28. Table xeroBankTransactions has data related to bank transactions.its column name bankAccountId  is equal to table xeroBankAccounts column id (foreign key).\r\n29. Table xeroBankAccounts has details about bank accounts and its primary key column id is used in table xeroBankTransfers in column name FromBankAccountId,ToBankAccountId.\r\n30. Table xeroContactAddresses has details about contacts we are using in xerotables . In table xeroBankTransactions the column contactId here refers to the primary key  id of this table. All the tables in xero having column contactId has foreign key of contact table column id.\r\n31. Table xeroCreditNotes has detailed information about credit note .\r\n32. Table xeroPayments contains information about payments for like payment type, bank amount ,status . The column invoiceid in this table is the primary key of invoice table means from that invoice id we can get the information from invoice table by matching it with invoice id.\r\n33. Table xeroInvoices contains information about invoices , amount due ,amount credited,amount paid .\r\n34. Table xeroContacts contains information about contacts.\r\n35. Table xeroQuotes contains quote id ,quote number .\r\n36. Table xeroPurchaseOrders contains detailed information about purchase orders.\r\n37. Table xeroReceipts contains information about receipt number , contact id and userId which is the foreign key from table users where it is equal to users table column id(primary key) .\r\n38. Table xeroUsers contains information about users\r\n39. Table xeroExpenseClaims contains status of claims amount paid ,amount due,payment due date and userid column is foreign key from user table equal to user table id.\r\n40. Table xeroItems contains details about items .\r\n41.In xeroBankTransactions table  when status column has value AUTHORISED it means it is done and authorised and if value is equal to DELETED it means the transactions are deleted .";

                    // string schema = "Provide proper sql queries with only provided columns and table names CREATE TABLE __EFMigrationsHistory (\r\nMigrationId nvarchar(150) NOT NULL,\r\nProductVersion nvarchar(32) NOT NUL\r\n);\r\nCREATE TABLE GoogleConfiguration (\r\nId int NOT NULL,\r\nName nvarchar(-1) NOT NULL,\r\nDescription nvarchar(-1) NULL,\r\nClientId nvarchar(-1) NOT NULL,\r\nClientSecret nvarchar(-1) NULL,\r\nCallBackUrl nvarchar(-1) NULL,\r\nScopes nvarchar(-1) NULL,\r\nTenantId nvarchar(-1) NULL,\r\nRefreshToken nvarchar(-1) NULL,\r\nRefreshTokenExpiresAt datetime2 NULL,\r\nAccessToken nvarchar(-1) NULL,\r\nAccessTokenExpiresAt datetime2 NULL,\r\nCreatedAt datetime2 NOT NULL,\r\nUpdateAt datetime2 NOT NUL\r\n);\r\nCREATE TABLE Documentslog (\r\nId int NOT NULL,\r\nName nvarchar(-1) NULL,\r\nDocumentId nvarchar(-1) NULL,\r\nPath nvarchar(-1) NULL,\r\nExtension nvarchar(-1) NULL,\r\nisDownloaded bit NOT NULL,\r\nisDeleted bit NOT NULL,\r\nCreatedAt datetime2 NOT NULL,\r\nUpdateAt datetime2 NOT NUL\r\n);\r\nCREATE TABLE ChatSession (\r\nId int NOT NUL\r\n);\r\nCREATE TABLE chatHistory (\r\nId int NOT NULL,\r\nUserId int NOT NULL,\r\nUserPrompt nvarchar(-1) NOT NULL,\r\nBotResponse nvarchar(-1) NOT NULL,\r\nChatSessionId int NOT NULL,\r\nTimestamp datetime2 NOT NULL,\r\nisDeleted bit NOT NUL\r\n);\r\n\r\nCompletion time: 2024-01-10T12:50:42.3921822+05:30\r\n";

                    var httpClient = new HttpClient();

                    var request = new
                    {
                        model = "gpt-3.5-turbo-16k",
                        messages = new[]
                        {
                new { role = "system", content = schema },
                new { role = "user", content = prompt },
                new { role = "assistant", content = "" }
            },
                        temperature = 0,
                        max_tokens = 1024,
                        top_p = 1,
                        frequency_penalty = 0,
                        presence_penalty = 0
                    };

                    var jsonRequest = JsonSerializer.Serialize(request);

                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openaiApiKey}");

                    var response = await httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var chatResponse = JsonSerializer.Deserialize<ChatCompletionViewModel>(jsonResponse);
                        if (chatResponse != null && chatResponse.choices.Length > 0)
                        {
                            var assistantContent = chatResponse.choices[0].message.content;
                            var queries = ExtractSqlQueries(assistantContent);


                            if (queries.Count > 0)
                            {

                                foreach (var query in queries)
                                {
                                    data = query;
                                }
                                List<Dictionary<string, object>> resultData = new List<Dictionary<string, object>>();


                                string rawSqlQuery = data;


                                var connectionString = "Server=ah-fencing-db-server.database.windows.net;Initial Catalog=ah-fencing-db;Persist Security Info=False;User ID=ah-fencing-admin;Password=freiuhYVIGYbkh7yebdkbvtBTVkubYVtuubcdrvebvityufr54444v6VTuyvyu;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";

                                using (var connection = new SqlConnection(connectionString))
                                {
                                    connection.Open();

                                    using (var command = connection.CreateCommand())

                                    // using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                                    {
                                        command.CommandText = rawSqlQuery;
                                        command.CommandType = CommandType.Text;

                                        dbContext.Database.OpenConnection();

                                        using (var result = command.ExecuteReader())
                                        {
                                            if (result.HasRows)
                                            {
                                                hasData = true;

                                            }
                                        }

                                    }

                                }
                            }
                            else
                            {
                                data = assistantContent;

                            }

                        }

                    }

                }
                else
                {
                    data = promptExist.botResponse;
                    hasData = true;
                }

                if (chatId == "new")
                {
                    var newSession = new Alloc8ChatSession();
                    dbContext.alloc8ChatSession.Add(newSession);
                    await dbContext.SaveChangesAsync();


                    var chatData = new Alloc8ChatGptHistory();
                    chatData.userPrompt = prompt;
                    chatData.botResponse = data;
                    chatData.userId = userId;
                    chatData.timestamp = DateTime.UtcNow;
                    chatData.chatSessionId = newSession.id;
                    dbContext.alloc8ChatGptHistory.Add(chatData);
                    await dbContext.SaveChangesAsync();
                    queryId = chatData.id;

                }
                else if (chatId == "old")
                {
                    var oldSession = dbContext.alloc8ChatSession.OrderByDescending(c => c.id).FirstOrDefault();
                    var chatData = new Alloc8ChatGptHistory();
                    chatData.userPrompt = prompt;
                    chatData.botResponse = data;
                    chatData.userId = userId;
                    chatData.timestamp = DateTime.UtcNow;
                    chatData.chatSessionId = oldSession.id;
                    dbContext.alloc8ChatGptHistory.Add(chatData);
                    await dbContext.SaveChangesAsync();
                    queryId = chatData.id;
                }
                else
                {
                    int oldChatId = Convert.ToInt32(chatId);
                    var oldSession = dbContext.alloc8ChatSession.OrderByDescending(c => c.id).FirstOrDefault();
                    var chatData = new Alloc8ChatGptHistory();
                    chatData.userPrompt = prompt;
                    chatData.botResponse = data;
                    chatData.userId = userId;
                    chatData.timestamp = DateTime.UtcNow;
                    chatData.chatSessionId = oldChatId;
                    dbContext.alloc8ChatGptHistory.Add(chatData);
                    await dbContext.SaveChangesAsync();
                    queryId = chatData.id;
                }

                string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";


                if (hasData == true)
                {
                    string url = baseUrl + "/LLM/Resource/" + queryId.ToString();
                    var chatRepsonse = dbContext.alloc8ChatGptHistory.FirstOrDefault(x => x.id == queryId);
                    chatRepsonse.url = url;
                    dbContext.alloc8ChatGptHistory.Update(chatRepsonse);
                    dbContext.SaveChanges();
                    return Json(new { status = 200, data = url });
                }
                else
                {
                    return Json(new { status = 400, data = queryId });
                }

            }
            catch (Exception e)

            {
                int queryId = 0;
                if (chatId == "new")
                {
                    var newSession = new Alloc8ChatSession();
                    dbContext.alloc8ChatSession.Add(newSession);
                    await dbContext.SaveChangesAsync();


                    var chatData = new Alloc8ChatGptHistory();
                    chatData.userPrompt = prompt;
                    chatData.botResponse = e.Message;
                    chatData.userId = userId;
                    chatData.timestamp = DateTime.UtcNow;
                    chatData.chatSessionId = newSession.id;
                    dbContext.alloc8ChatGptHistory.Add(chatData);
                    await dbContext.SaveChangesAsync();
                    queryId = chatData.id;

                }
                else if (chatId == "old")
                {
                    var oldSession = dbContext.alloc8ChatSession.OrderByDescending(c => c.id).FirstOrDefault();
                    var chatData = new Alloc8ChatGptHistory();
                    chatData.userPrompt = prompt;
                    chatData.botResponse = e.Message;
                    chatData.userId = userId;
                    chatData.timestamp = DateTime.UtcNow;
                    chatData.chatSessionId = oldSession.id;
                    dbContext.alloc8ChatGptHistory.Add(chatData);
                    await dbContext.SaveChangesAsync();
                    queryId = chatData.id;
                }
                else
                {
                    int oldChatId = Convert.ToInt32(chatId);
                    var oldSession = dbContext.alloc8ChatSession.OrderByDescending(c => c.id).FirstOrDefault();
                    var chatData = new Alloc8ChatGptHistory();
                    chatData.userPrompt = prompt;
                    chatData.botResponse = e.Message;
                    chatData.userId = userId;
                    chatData.timestamp = DateTime.UtcNow;
                    chatData.chatSessionId = oldChatId;
                    dbContext.alloc8ChatGptHistory.Add(chatData);
                    await dbContext.SaveChangesAsync();
                    queryId = chatData.id;
                }

                return Json(new { status = 400, data = queryId });
            }
        }

        public async Task<List<Alloc8ChatSessionHistory>> getAhFenicngChatSessionGroupsAsync()
        {
            var userId = string.Empty;
            var user = await _userManager.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefaultAsync();

            if (user != null)
            {
                userId = user.Id;
            }
            if (string.IsNullOrEmpty(userId))
            {
                return new List<Alloc8ChatSessionHistory>();
            }

            var chatHistoryList = await dbContext.alloc8ChatGptHistory
                                                  .Where(x => x.isDeleted == false && x.userId == userId)
                                                  .ToListAsync();

            var chatSessionGroupsData =
                chatHistoryList.GroupBy(history => history.chatSessionId)
                               .Select(group => new Alloc8ChatSessionHistory
                               {
                                   chatSessionId = group.Key,
                                   lastUserPrompt = group.OrderByDescending(history => history.timestamp)
                                                       .Select(history => history.userPrompt)
                                                       .FirstOrDefault()
                               })
                               .ToList();

            return chatSessionGroupsData;
        }
    }
}
