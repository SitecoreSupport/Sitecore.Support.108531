using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Sitecore.Analytics;
using Sitecore.Analytics.Data.Items;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Globalization;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Xml;

namespace Sitecore.Support.Shell.Applications.Analytics.TrackingField
{
  [UsedImplicitly]
  public class TrackingFieldPage: Sitecore.Shell.Applications.Analytics.TrackingField.TrackingFieldPage
  {
    protected override void Render([NotNull] XDocument doc)
    {
      Assert.ArgumentNotNull(doc, "doc");

      var root = doc.Root;
      if (root != null)
      {
        if (root.GetAttributeValue("ignore") == "1")
        {
          this.Ignore.Checked = true;
        }
      }

      this.RenderCampaigns(doc);
      this.RenderFailures(doc);
      this.RenderEvents(doc);
    }

    private void RenderCampaigns([NotNull] XDocument doc)
    {
      Assert.ArgumentNotNull(doc, "doc");

      var selected = new List<string>();

      foreach (var element in doc.Descendants("campaign"))
      {
        selected.Add(element.GetAttributeValue("id"));
      }

      var page = GetPage();
      if (page == null || page.IsPostBack)
      {
        return;
      }

      var output = new HtmlTextWriter(new StringWriter());


      foreach (Item child in Tracker.DefinitionItems.Campaigns.Where(c => c.IsDeployed))
      {
        this.RenderCampaigns(output, selected, child);
      }

      this.CampaignList.InnerHtml = output.InnerWriter.ToString();
    }

    private void RenderCampaigns([NotNull] HtmlTextWriter output, [NotNull] List<string> selected, [NotNull] Item campaign)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(selected, "selected");
      Debug.ArgumentNotNull(campaign, "campaign");

      if (campaign.TemplateID == Marketing.Definitions.Campaigns.WellKnownIdentifiers.CampaignActivityDefinitionTemplateId)
      {
        var hidden = campaign["Hidden"] == "1";
        if (hidden)
        {
          return;
        }

        var name = campaign.Fields["Title"].GetValue(false, false);
        if (string.IsNullOrEmpty(name))
        {
          name = campaign.Name;
        }

        var id = "campaign_" + ShortID.Encode(campaign.ID);
        var isChecked = selected.IndexOf(campaign.ID.ToString()) >= 0 ? " checked=\"checked\"" : string.Empty;

        output.Write("<div class=\"scCampaignContainer\">");
        output.Write("<input type=\"checkbox\" id=\"" + id + "\" name=\"" + id + "\" " + isChecked + "/>");
        output.Write("<label for=\"" + id + "\">");
        output.Write(name);
        output.Write("</label>");
        output.Write("</div>");

        return;
      }

      foreach (Item child in campaign.Children)
      {
        this.RenderCampaigns(output, selected, child);
      }

    }

    private void RenderEvents([NotNull] XDocument doc)
    {
      Assert.ArgumentNotNull(doc, "doc");

      var selected = new List<string>();

      foreach (var element in doc.Descendants("event"))
      {
        selected.Add(element.GetAttributeValue("name"));
      }

      var checkBoxList = new CheckBoxList
      {
        ID = "EventsCheckBoxList"
      };

      this.EventsList.Controls.Add(checkBoxList);

      var page = GetPage();
      if (page == null || page.Page.IsPostBack)
      {
        return;
      }

      var pageEventDefinitions = Tracker.DefinitionItems.AllPageEvents.Where(e => e.IsDeployed && !e.IsSystem && !e.IsGoal && !e.IsFailure).OrderBy(e => e.DisplayName);
      RenderCheckBoxList(checkBoxList, pageEventDefinitions, selected);
    }

    private void RenderFailures([NotNull] XDocument doc)
    {
      Assert.ArgumentNotNull(doc, "doc");

      var selected = new List<string>();

      foreach (var element in doc.Descendants("event"))
      {
        selected.Add(element.GetAttributeValue("name"));
      }

      var checkBoxList = new CheckBoxList
      {
        ID = "FailuresCheckBoxList"
      };

      this.FailuresList.Controls.Add(checkBoxList);

      var page = GetPage();
      if (page == null || page.IsPostBack)
      {
        return;
      }

      var pageEventDefinitions = Tracker.DefinitionItems.AllPageEvents.Where(e => e.IsDeployed && !e.IsSystem && e.IsFailure).OrderBy(e => e.DisplayName);

      RenderCheckBoxList(checkBoxList, pageEventDefinitions, selected);
    }
  }
}