/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Property } from "./Property";

export class ItemType {
	name: string;
	technicalName: string;
	properties: Property[];

	static FIELD_name: string = 'name';	
	static FIELD_technicalName: string = 'technicalName';	
	static FIELD_properties: string = 'properties';

	static getForm(formBuilder: FormBuilder, current: ItemType): FormGroup {
		return formBuilder.group({
			name: [current && current.name],
			technicalName: [current && current.technicalName],
			properties: formBuilder.array(!!!(current && current.properties) ? [] : current.properties.map(item => Property.getForm(formBuilder, item))),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: ItemType) {
		form.removeControl('properties');
		form.addControl('properties', formBuilder.array(!!!(current && current.properties) ? [] : current.properties.map(item => Property.getForm(formBuilder, item))),);
	}
}
