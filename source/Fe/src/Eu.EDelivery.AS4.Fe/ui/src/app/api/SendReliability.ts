/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ReceptionAwareness } from "./ReceptionAwareness";

export class SendReliability {
	receptionAwareness: ReceptionAwareness;

	static FIELD_receptionAwareness: string = 'receptionAwareness';

	static getForm(formBuilder: FormBuilder, current: SendReliability): FormGroup {
		return formBuilder.group({
			receptionAwareness: ReceptionAwareness.getForm(formBuilder, current && current.receptionAwareness),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendReliability) {
		ReceptionAwareness.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_receptionAwareness), current && current.receptionAwareness);
	}
}
