import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SendingPmode } from './SendingPmode';
import { SendingProcessingModeForm } from './SendingProcessingModeForm';
import { FormWrapper } from './../common/form.service';
import { SendingProcessingMode } from './SendingProcessingMode';

export class SendingPmodeForm {
    public static getForm(formWrapper: FormWrapper, current: SendingPmode): FormWrapper {
        let previousDynamicDiscoveryEnabled: boolean | null = null;
        return formWrapper
            .group({
                type: [current && current.type],
                name: [current && current.name],
                pmode: SendingProcessingModeForm.getForm(formWrapper.subForm('pmode'), current && current.pmode).form,
                [SendingPmode.FIELD_isDynamicDiscoveryEnabled]: [current && current.isDynamicDiscoveryEnabled]
            })
            .onChange<boolean>(SendingPmode.FIELD_isDynamicDiscoveryEnabled, (value, wrapper) => {
                if (previousDynamicDiscoveryEnabled === value) {
                    return;
                }

                previousDynamicDiscoveryEnabled = value;

                // tslint:disable-next-line:max-line-length
                const pushConfig = wrapper.form.get(`${SendingPmode.FIELD_pmode}.${SendingProcessingMode.FIELD_pushConfiguration}`);
                const dynamicDiscovery = wrapper.form.get(`${SendingPmode.FIELD_pmode}.${SendingProcessingMode.FIELD_dynamicDiscovery}`);

                if (!value) {
                    pushConfig!.enable();
                    dynamicDiscovery!.disable();
                    wrapper.reApplyHandlers();
                } else {
                    pushConfig!.disable();
                    dynamicDiscovery!.enable();
                    wrapper.reApplyHandlers();
                }
            })
            .triggerHandler(SendingPmode.FIELD_isDynamicDiscoveryEnabled, current && current.isDynamicDiscoveryEnabled);
    }
}
