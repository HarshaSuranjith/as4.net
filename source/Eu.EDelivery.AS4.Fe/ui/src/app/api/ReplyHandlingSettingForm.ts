import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { PushConfigurationForm } from './PushConfigurationForm';
import { ReceiveErrorHandlingForm } from './ReceiveErrorHandlingForm';
import { ReceiveReceiptHandlingForm } from './ReceiveReceiptHandlingForm';
import { ReplyHandlingSetting } from './ReplyHandlingSetting';
import { RetryReliabilityForm } from './RetryReliabilityForm';
import { SigningForm } from './SigningForm';

export class ReplyHandlingSettingForm {
  public static getForm(
    formBuilder: FormWrapper,
    current: ReplyHandlingSetting,
    path: string,
    runtime: ItemType[]
  ): FormWrapper {
    return formBuilder
      .group({
        [ReplyHandlingSetting.FIELD_replyPattern]: [
          formBuilder.createFieldValue(
            current,
            ReplyHandlingSetting.FIELD_replyPattern,
            path,
            0,
            runtime
          )
        ],
        [ReplyHandlingSetting.FIELD_receiptHandling]: ReceiveReceiptHandlingForm.getForm(
          formBuilder.subForm(ReplyHandlingSetting.FIELD_receiptHandling),
          current && current.receiptHandling,
          `${path}.${ReplyHandlingSetting.FIELD_receiptHandling}`,
          runtime
        ).form,
        [ReplyHandlingSetting.FIELD_receiveErrorHandling]: ReceiveErrorHandlingForm.getForm(
          formBuilder.subForm(ReplyHandlingSetting.FIELD_receiveErrorHandling),
          current && current.errorHandling,
          `${path}.${ReplyHandlingSetting.FIELD_receiveErrorHandling}`,
          runtime
        ).form,
        [ReplyHandlingSetting.FIELD_piggyBackReliability]: RetryReliabilityForm.getForm(
          formBuilder.subForm(ReplyHandlingSetting.FIELD_piggyBackReliability),
          current && current.piggyBackReliability,
          `${path}.${ReplyHandlingSetting.FIELD_piggyBackReliability}`,
          runtime
        ).form,
        [ReplyHandlingSetting.FIELD_responseConfiguration]: PushConfigurationForm.getForm(
          formBuilder.subForm(ReplyHandlingSetting.FIELD_responseConfiguration),
          current && current.responseConfiguration,
          runtime,
          `${path}.${ReplyHandlingSetting.FIELD_responseConfiguration}`
        ).form,
        [ReplyHandlingSetting.FIELD_responseSigning]: SigningForm.getForm(
          formBuilder.subForm(ReplyHandlingSetting.FIELD_responseSigning),
          current && current.responseSigning,
          `${path}.${ReplyHandlingSetting.FIELD_responseSigning}`,
          runtime
        ).form
      })
      .onChange<number>(
        ReplyHandlingSetting.FIELD_replyPattern,
        (field, wrapper) => {
          const excepts = [
            ReplyHandlingSetting.FIELD_replyPattern,
            ReplyHandlingSetting.FIELD_piggyBackReliability,
            ReplyHandlingSetting.FIELD_receiptHandling,
            ReplyHandlingSetting.FIELD_receiveErrorHandling,
            ReplyHandlingSetting.FIELD_responseSigning
          ];
          if (field === 1) {
            wrapper.enable(excepts);
          } else {
            wrapper.disable(excepts);
          }
        }
      )
      .triggerHandler(
        ReplyHandlingSetting.FIELD_replyPattern,
        current && current.replyPattern
      );
  }
}
