using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UITryoutsRegisterView : UIScreenView
	{
		public UINavigation submitButtonNav;

		public FadeComponent submitButtonFade;

		public ListComponent listField;

		public RectTransform sendingIndicator;

		public DRLInputFieldView nameField;

		public DRLInputFieldView emailField;

		public DRLStepperView ageField;

		public DRLInputFieldView ageInputField;

		public DRLInputFieldView areaCodeInputField;

		public DRLInputFieldView phoneNumberInputField;

		public DRLTextAssetStepperView genderField;

		public DRLDropdownView countryDropdown;

		public GameObject quebecDisclaimerLabel;

		public DRLTextAssetStepperView nonFpvDroneSkillField;

		public DRLTextAssetStepperView nonFpvDroneSkillYearsField;

		public DRLTextAssetStepperView fpvDroneSkillField;

		public DRLTextAssetStepperView fpvDroneSkillYearsField;

		public DRLTextAssetStepperView fpvDronePreferenceField;

		public RectTransform realLifeCompetitionFieldLabel;

		public DRLTextAssetStepperView realLifeCompetitionField;

		public DRLTextAssetStepperView droneBuiltOwnField;

		public DRLTextAssetStepperView drlWatchField;

		public RectTransform multiGPFieldLabel;

		public DRLTextAssetStepperView multiGPField;

		public DRLTextAssetStepperView americanCitizenField;

		public RectTransform usMilitaryFieldLabel;

		public DRLTextAssetStepperView usMilitaryField;

		public RectTransform amaMemberFieldLabel;

		public DRLTextAssetStepperView amaMemberField;

		public List<UINavigation> navigationElements;

		public DRLCampaign data;

		public int age
		{
			get
			{
				int result = -1;
				int.TryParse(ageInputField.field.text, out result);
				return result;
			}
			set
			{
				ageInputField.field.enabled = false;
				ageInputField.field.text = value.ToString();
				ageInputField.field.enabled = true;
			}
		}

		public void Set(DRLCampaign p_data)
		{
			data = p_data;
		}

		public void Set(CampaignRegisterInfo p_data)
		{
			SetCountries();
			CampaignRegisterInfo campaignRegisterInfo = ((p_data == null) ? new CampaignRegisterInfo() : p_data);
			age = Mathf.Max(ageField.min, campaignRegisterInfo.age);
			emailField.field.text = campaignRegisterInfo.email;
			nameField.field.text = campaignRegisterInfo.name;
			areaCodeInputField.text = campaignRegisterInfo.area;
			if (string.IsNullOrEmpty(areaCodeInputField.text))
			{
				areaCodeInputField.text = "1";
			}
			phoneNumberInputField.text = campaignRegisterInfo.phone;
			countryDropdown.Select(campaignRegisterInfo.country);
			genderField.SetValue(campaignRegisterInfo.gender);
			americanCitizenField.SetValue(campaignRegisterInfo.americanCitizen);
			nonFpvDroneSkillField.SetValue(campaignRegisterInfo.experienceNonFPV);
			nonFpvDroneSkillYearsField.SetValue(campaignRegisterInfo.experienceNonFPVYears);
			fpvDroneSkillField.SetValue(campaignRegisterInfo.experienceFPV);
			fpvDroneSkillYearsField.SetValue(campaignRegisterInfo.experienceFPVYears);
			fpvDronePreferenceField.SetValue(campaignRegisterInfo.experiencePreferenceFPV);
			realLifeCompetitionField.SetValue(campaignRegisterInfo.experienceRealLifeRacing);
			droneBuiltOwnField.SetValue(campaignRegisterInfo.experienceBuiltOwnDrone);
			drlWatchField.SetValue(campaignRegisterInfo.affiliationWatchDRL);
			multiGPField.SetValue(campaignRegisterInfo.affiliationMultiGP);
			usMilitaryField.SetValue(campaignRegisterInfo.affiliationMilitary);
			amaMemberField.SetValue(campaignRegisterInfo.affiliationAMA);
		}

		private void SetCountries()
		{
			string[] array = "U.S.,Afghanistan,Albania,Algeria,Andorra,Angola,Antigua & Deps,Argentina,Armenia,Australia,Austria,Azerbaijan,Bahamas,Bahrain,Bangladesh,Barbados,Belarus,Belgium,Belize,Benin,Bhutan,Bolivia,Bosnia Herzegovina,Botswana,Brazil,Brunei,Bulgaria,Burkina,Burundi,Cambodia,Cameroon,Canada,Cape Verde,Central African Rep,Chad,Chile,China,Colombia,Comoros,Congo,Costa Rica,Croatia,Cuba,Cyprus,Czech Republic,Denmark,Djibouti,Dominica,Dominican Republic,East Timor,Ecuador,Egypt,El Salvador,Equatorial Guinea,Eritrea,Estonia,Ethiopia,Fiji,Finland,France,Gabon,Gambia,Georgia,Germany,Ghana,Greece,Grenada,Guatemala,Guinea,Guinea-Bissau,Guyana,Haiti,Honduras,Hungary,Iceland,India,Indonesia,Iran,Iraq,Ireland,Israel,Italy,Ivory Coast,Jamaica,Japan,Jordan,Kazakhstan,Kenya,Kiribati,Korea North,Korea South,Kosovo,Kuwait,Kyrgyzstan,Laos,Latvia,Lebanon,Lesotho,Liberia,Libya,Liechtenstein,Lithuania,Luxembourg,Macedonia,Madagascar,Malawi,Malaysia,Maldives,Mali,Malta,Marshall Islands,Mauritania,Mauritius,Mexico,Micronesia,Moldova,Monaco,Mongolia,Montenegro,Morocco,Mozambique,Myanmar,Namibia,Nauru,Nepal,Netherlands,New Zealand,Nicaragua,Niger,Nigeria,Norway,Oman,Pakistan,Palau,Panama,Papua New Guinea,Paraguay,Peru,Philippines,Poland,Portugal,Qatar,Romania,Russian Federation,Rwanda,St Kitts & Nevis,St Lucia,St. Vincent & the Grenadines,Samoa,San Marino,Sao Tome & Principe,Saudi Arabia,Senegal,Serbia,Seychelles,Sierra Leone,Singapore,Slovakia,Slovenia,Solomon Islands,Somalia,South Africa,South Sudan,Spain,Sri Lanka,Sudan,Suriname,Swaziland,Sweden,Switzerland,Syria,Taiwan,Tajikistan,Tanzania,Thailand,Togo,Tonga,Trinidad & Tobago,Tunisia,Turkey,Turkmenistan,Tuvalu,Uganda,Ukraine,U.A.E.,U.K.,Uruguay,Uzbekistan,Vanuatu,Vatican City,Venezuela,Vietnam,Yemen,Zambia,Zimbabwe".Split(',');
			countryDropdown.Clear();
			string[] array2 = array;
			foreach (string text in array2)
			{
				countryDropdown.Add(new Dropdown.OptionData(text));
			}
			countryDropdown.Select(0);
		}

		public CampaignRegisterInfo Get()
		{
			return new CampaignRegisterInfo
			{
				guid = (data ? data.guid : ""),
				age = age,
				email = emailField.field.text,
				area = areaCodeInputField.text,
				phone = phoneNumberInputField.text,
				country = countryDropdown.Value().text,
				gender = genderField.value,
				name = nameField.field.text,
				americanCitizen = americanCitizenField.value,
				experienceNonFPV = nonFpvDroneSkillField.value,
				experienceNonFPVYears = nonFpvDroneSkillYearsField.value,
				experienceFPV = fpvDroneSkillField.value,
				experienceFPVYears = fpvDroneSkillYearsField.value,
				experiencePreferenceFPV = fpvDronePreferenceField.value,
				experienceRealLifeRacing = realLifeCompetitionField.value,
				experienceBuiltOwnDrone = droneBuiltOwnField.value,
				affiliationWatchDRL = drlWatchField.value,
				affiliationMultiGP = multiGPField.value,
				affiliationMilitary = usMilitaryField.value,
				affiliationAMA = amaMemberField.value
			};
		}

		public void ShowSendingIndicator(bool p_show)
		{
			if ((bool)sendingIndicator)
			{
				sendingIndicator.gameObject.SetActive(p_show);
			}
		}

		public void SetSubmitAvailable(bool p_flag)
		{
			submitButtonNav.enabled = p_flag;
			submitButtonFade.Fade(p_flag ? 1f : 0.1f);
		}

		public void ClearErrors()
		{
			UIFieldErrorIndicator component = ageField.GetComponent<UIFieldErrorIndicator>();
			if ((bool)component)
			{
				component.Hide();
			}
			component = ageInputField.GetComponent<UIFieldErrorIndicator>();
			if ((bool)component)
			{
				component.Hide();
			}
			component = nameField.GetComponent<UIFieldErrorIndicator>();
			if ((bool)component)
			{
				component.Hide();
			}
			component = emailField.GetComponent<UIFieldErrorIndicator>();
			if ((bool)component)
			{
				component.Hide();
			}
			component = countryDropdown.GetComponent<UIFieldErrorIndicator>();
			if ((bool)component)
			{
				component.Hide();
			}
			component = areaCodeInputField.GetComponent<UIFieldErrorIndicator>();
			if ((bool)component)
			{
				component.Hide();
			}
			component = phoneNumberInputField.GetComponent<UIFieldErrorIndicator>();
			if ((bool)component)
			{
				component.Hide();
			}
		}
	}
}
