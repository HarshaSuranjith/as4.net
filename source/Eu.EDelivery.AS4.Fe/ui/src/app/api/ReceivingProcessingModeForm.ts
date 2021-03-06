import { Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { MessageHandlingForm } from './MessageHandlingForm';
import { MessagePackagingForm } from './MessagePackagingForm';
import { ReceivehandlingForm } from './ReceivehandlingForm';
import { ReceiveReliabilityForm } from './ReceiveReliabilityForm';
import { ReceiveSecurityForm } from './ReceiveSecurityForm';
import { ReceivingProcessingMode } from './ReceivingProcessingMode';
import { ReplyHandlingSettingForm } from './ReplyHandlingSettingForm';

export class ReceivingProcessingModeForm {
    public static getForm(formBuilder: FormWrapper, current: ReceivingProcessingMode, runtime: ItemType[], path: string = 'receivingprocessingmode'): FormWrapper {
        return formBuilder.group({
            [ReceivingProcessingMode.FIELD_id]: [formBuilder.createFieldValue(current, ReceivingProcessingMode.FIELD_id, path, null, runtime), Validators.required],
            [ReceivingProcessingMode.FIELD_reliability]: ReceiveReliabilityForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_reliability), current && current.reliability, `${path}.${ReceivingProcessingMode.FIELD_reliability}`, runtime).form,
            [ReceivingProcessingMode.FIELD_replyHandling]: ReplyHandlingSettingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_replyHandling), current && current.replyHandling, `${path}.${ReceivingProcessingMode.FIELD_replyHandling}`, runtime).form,
            [ReceivingProcessingMode.FIELD_exceptionHandling]: ReceivehandlingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_exceptionHandling), current && current.exceptionHandling, `${path}.${ReceivingProcessingMode.FIELD_exceptionHandling}`, runtime).form,
            [ReceivingProcessingMode.FIELD_security]: ReceiveSecurityForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_security), current && current.security, `${path}.${ReceivingProcessingMode.FIELD_security}`, runtime).form,
            [ReceivingProcessingMode.FIELD_messagePackaging]: MessagePackagingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_messagePackaging), current && current.messagePackaging, `${path}.${ReceivingProcessingMode.FIELD_messagePackaging}`, runtime).form,
            [ReceivingProcessingMode.FIELD_messageHandling]: MessageHandlingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_messageHandling), current && current.messageHandling, runtime, `${path}.${ReceivingProcessingMode.FIELD_messageHandling}`).form
        });
    }
}
