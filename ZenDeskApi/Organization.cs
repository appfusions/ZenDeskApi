using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using ZenDeskApi.Model;

namespace ZenDeskApi
{
    public partial class ZenDeskApi
    {
        private string _organizations = "organizations";

        public List<Organization> GetOrganizations()
        {
            var organizations = new List<Organization>();

            try
            {
                var page = 1;
                var pageOfOrgs = new List<Organization>();

                //Try getting the tickets for all of the pages
                while (page == 1 || pageOfOrgs.Count > 0)
                {
                    pageOfOrgs = GetOrganizationssByPage(page);
                    organizations.AddRange(pageOfOrgs);

                    page++;
                }
            }
            //There were no more pages so just go on
            catch (ArgumentNullException ex)
            { }

            return organizations;
        }

        private List<Organization> GetOrganizationssByPage(int page)
        {
            var request = new ZenRestRequest
            {
                Method = Method.GET,
                Resource = string.Format("{0}.xml", _organizations)
            };

            request.AddParameter("page", page.ToString());

            return Execute<List<Organization>>(request);
        }

        public Organization GetOrganizationById(int id)
        {
            var request = new ZenRestRequest
            {
                Method = Method.GET,
                Resource = string.Format("{0}/{1}.xml", _organizations, id)
            };

            return Execute<Organization>(request);            
        }      

        public int CreateOrUpdateOrganization(string name, string defaultSite)
        {
            return CreateOrUpdateOrganization(new Organization() { Name = name, Default = defaultSite });
        }

        public int CreateOrUpdateOrganization(Organization newOrg)
        {
            var orgs = GetOrganizations();
            var curOrg = orgs.Where(x => x.Name == newOrg.Name);

            if (curOrg.Count() > 0)
            {
                newOrg.Id = curOrg.First().Id;

                //If it couldn't be updated
                if (!UpdateOrganization(newOrg))
                    return -1;

                return curOrg.First().Id;
            }

            var request = new ZenRestRequest
            {
                Method = Method.POST,
                Resource = _organizations + ".xml"
            };

            request.AddBody(newOrg);

            var res = Execute(request);

            return GetIdFromLocationHeader(res);
        }

        public bool UpdateOrganization(Organization org)
        {
            var request = new ZenRestRequest
            {
                Method = Method.PUT,
                Resource = string.Format("{0}/{1}.xml", _organizations, org.Id)
            };

            request.AddBody(org);

            var res = Execute(request);

            return res.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public bool DestroyOrganization(int orgId)
        {
            var request = new ZenRestRequest
            {
                Method = Method.DELETE,
                Resource = string.Format("{0}/{1}.xml", _organizations, orgId)
            };

            var res = Execute(request);

            return res.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
