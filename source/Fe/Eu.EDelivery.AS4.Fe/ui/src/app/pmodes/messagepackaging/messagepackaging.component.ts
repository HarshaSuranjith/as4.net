import { FormGroup } from '@angular/forms';
import { Component, Input } from '@angular/core';

@Component({
    selector: 'as4-message-packaging',
    template: `
        <div [formGroup]="form">
            <ng-container *ngIf="isSendingType">
                <as4-input label="Use AS4 compression" runtimetooltip="messagepackaging.useas4compression">
                    <input type="checkbox" formControlName="useAS4Compression" />
                </as4-input>
                <as4-input label="Use multi hop" runtimetooltip="messagepackaging.ismultihop">
                    <input type="checkbox" formControlName="isMultiHop" />
                </as4-input>
                <as4-input label="Include pmode id" runtimetooltip="messagepackaging.includepmodeid">
                    <input type="checkbox" formControlName="includePModeId" />
                </as4-input>
                <as4-input label="MPC" runtimetooltip="messagepackaging.mpc">
                    <input type="text" formControlName="mpc" />
                </as4-input>         
            </ng-container>
            <div class="sub-header-1">Party info</div>
            <as4-party label="From party" [group]="form.get('partyInfo.fromParty')" runtimetooltip="messagepackaging.partyinfo.fromparty"></as4-party>
            <as4-party label="To party" [group]="form.get('partyInfo.toParty')" runtimetooltip="messagepackaging.partyinfo.toparty"></as4-party>           
            <div formGroupName="collaborationInfo">
                <div class="sub-header-1">Collaboration info</div>
                <as4-input isLabelBold="true" label="Action" runtimetooltip="collaborationinfo.action">
                    <input type="text" formControlName="action" />
                </as4-input>
                <as4-input isLabelBold="true" label="Service" runtimetooltip="collaborationinfo.service"> 
                    <as4-columns formGroupName="service">
                        <input type="text" placeholder="value" formControlName="value" />
                        <input type="text" placeholder="type" formControlName="type" />
                    </as4-columns>
                </as4-input>
                <ng-container formGroupName="agreementReference">
                    <as4-input isLabelBold="true" label="Agreement reference" runtimetooltip="collaborationinfo.agreementreference">
                        <as4-columns>
                            <input type="text" formControlName="value" placeholder="value" />
                            <input type="text" formControlName="type" placeholder="type" />
                        </as4-columns>
                    </as4-input>
                </ng-container>
            </div>
        </div>
    `
})
export class MessagePackagingComponent {
    @Input() public form: FormGroup;
    @Input() public disabled: boolean;
    @Input() public isSendingType: boolean = true;
}
