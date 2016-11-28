/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';
import { SettingsDatabase } from './SettingsDatabase'
import { CustomSettings } from './CustomSettings'
import { SettingsAgents } from './SettingsAgents'

export class Settings {
	idFormat: string;
	certificateStoreName: string;
	database: SettingsDatabase;
	customSettings: CustomSettings;
	agents: SettingsAgents;

	static getForm(formBuilder: FormBuilder, current: Settings): FormGroup {
		return formBuilder.group({
				idFormat: [current && current.idFormat],
				certificateStoreName: [current && current.certificateStoreName],
				database: SettingsDatabase.getForm(formBuilder, current && current.database),
				customSettings: CustomSettings.getForm(formBuilder, current && current.customSettings),
				agents: SettingsAgents.getForm(formBuilder, current && current.agents),
		});
	}
}
