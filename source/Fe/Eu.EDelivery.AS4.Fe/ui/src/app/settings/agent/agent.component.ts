import { CanComponentDeactivate } from './../../common/candeactivate.guard';
import { Observer } from 'rxjs/Observer';
import { removeNgStyles } from '@angularclass/hmr';
import { ActivatedRoute } from '@angular/router';
import { Component, Input, OnDestroy, ViewChild, ElementRef, NgZone } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { NgForm, FormBuilder, FormGroup, FormArray } from '@angular/forms';

import { ModalService } from './../../common/modal/modal.service';
import { RuntimeStore } from '../runtime.store';
import { Setting } from './../../api/Setting';
import { Steps } from './../../api/Steps';
import { Step } from './../../api/Step';
import { Transformer } from './../../api/Transformer';
import { Receiver } from './../../api/Receiver';
import { ReceiverComponent } from '../receiver.component';
import { SettingsAgent } from '../../api/SettingsAgent';
import { SettingsAgentForm } from '../../api/SettingsAgentForm';
import { SettingsService } from '../settings.service';
import { SettingsStore } from '../settings.store';
import { DialogService } from './../../common/dialog.service';
import { ItemType } from './../../api/ItemType';
import { Property } from './../../api/Property';
import { FormBuilderExtended, FormWrapper } from './../../common/form.service';

@Component({
    selector: 'as4-agent-settings',
    templateUrl: './agent.component.html'
})
export class AgentSettingsComponent implements OnDestroy, CanComponentDeactivate {
    public settings: SettingsAgent[] = new Array<SettingsAgent>();
    public collapsed: boolean = true;

    public get currentAgent(): SettingsAgent {
        return this._currentAgent;
    }
    public set currentAgent(agent: SettingsAgent) {
        this._currentAgent = agent;
    }
    public transformers: ItemType[];
    public isNewMode: boolean = false;
    public newName: string;
    public actionType: string | number = 0;

    public form: FormGroup;
    @Input() public title: string;
    @Input() public agent: string;

    private _currentAgent: SettingsAgent;
    private _settingsStoreSubscription: Subscription;
    private _runtimeStoreSubscription: Subscription;
    private _formWrapper: FormWrapper;

    constructor(private settingsStore: SettingsStore, private settingsService: SettingsService, private activatedRoute: ActivatedRoute,
        private runtimeStore: RuntimeStore, private dialogService: DialogService, private modalService: ModalService, private formBuilder: FormBuilderExtended) {
        this._formWrapper = this.formBuilder.get();
        this.form = SettingsAgentForm.getForm(this._formWrapper, null).build();
        this._formWrapper.disable();
        if (!!this.activatedRoute.snapshot.data['type']) {
            this.title = `${this.activatedRoute.snapshot.data['title']} agent`;
            this.collapsed = false;
            this.agent = this.activatedRoute.snapshot.data['type'];
        }

        this._runtimeStoreSubscription = this.runtimeStore
            .changes
            .filter((x) => x != null)
            .subscribe((result) => {
                this.transformers = result.transformers;
            });

        this._settingsStoreSubscription = this.settingsStore
            .changes
            .filter((result) => !!result && !!result.Settings && !!result.Settings.agents[this.agent])
            .map((result) => result.Settings.agents[this.agent])
            .distinctUntilChanged()
            .subscribe((result) => {
                this.settings = result;
                this.currentAgent = result.find((agt) => agt.name === this.form.value.name);
                if (!!this.currentAgent) {
                    this.form = SettingsAgentForm.getForm(this._formWrapper, this.currentAgent).build();
                }
            });
    }
    public addAgent() {
        this.modalService
            .show('new-agent')
            .filter((result) => result)
            .subscribe(() => {
                if (!!this.newName) {
                    if (this.messageIfExists(this.newName)) {
                        return;
                    }
                    let newAgent;
                    if (+this.actionType !== -1) {
                        newAgent = Object.assign({}, this.settings.find(agt => agt.name === this.actionType));
                    } else {
                        newAgent = new SettingsAgent();
                    }
                    this.settings.push(newAgent);
                    this.currentAgent = newAgent;
                    this.currentAgent.name = this.newName;
                    this.form = SettingsAgentForm.getForm(this._formWrapper, this.currentAgent).form;
                    this.isNewMode = true;
                    this.form.reset(newAgent);
                    this.form.patchValue({ [SettingsAgent.FIELD_name]: this.newName });
                    this.form.markAsDirty();
                }
            });
    }
    public selectAgent(selectedAgent: string = null): boolean {
        let select = () => {
            this.isNewMode = false;
            this.currentAgent = this.settings.find((agent) => agent.name === selectedAgent);
            this.form = SettingsAgentForm.getForm(this._formWrapper, this.currentAgent).build(!!!this.currentAgent);
        };

        if (this.form.dirty) {
            this.dialogService
                .confirmUnsavedChanges()
                .filter((result) => result)
                .subscribe(() => {
                    if (this.isNewMode) {
                        this.settings = this.settings.filter((agent) => agent !== this.currentAgent);
                    } else {
                        select();
                    }
                });

            return false;
        }

        select();
        return true;
    }
    public save() {
        if (!this.form.valid) {
            this.dialogService.message('Input is not valid, please correct the invalid fields');
            return;
        }
        let obs;
        if (!this.isNewMode) {
            obs = this.settingsService.updateAgent(this.form.value, this.currentAgent.name, this.agent);
        } else {
            obs = this.settingsService.createAgent(this.form.value, this.agent);
        }
        obs.subscribe((result) => {
            if (result) {
                this.isNewMode = false;
                this.form.markAsPristine();
            }
        });
    }
    public reset() {
        if (this.isNewMode) {
            this.settings = this.settings.filter((agent) => agent !== this.currentAgent);
            this.currentAgent = undefined;
        }
        this.isNewMode = false;
        this.form = SettingsAgentForm.getForm(this._formWrapper, this.currentAgent).build();
        this.form.reset(this.currentAgent);
    }
    public rename() {
        this.dialogService
            .prompt('Please enter a new name', 'Rename')
            .subscribe((name) => {
                if (this.messageIfExists(name)) {
                    return;
                }
                if (!!this.currentAgent && !!name) {
                    this.form.patchValue({ [SettingsAgent.FIELD_name]: name });
                    this.form.markAsDirty();
                }
            });
    }
    public delete() {
        this.dialogService
            .confirm('Are you sure you want to delete the agent', 'Delete agent')
            .filter((result) => result)
            .subscribe((result) => {
                this.form.markAsPristine();
                if (this.isNewMode) {
                    this.settings = this.settings.filter((agent) => agent.name !== this.currentAgent.name);
                    this.selectAgent();
                    return;
                }

                this.settingsService.deleteAgent(this.currentAgent, this.agent);
            });
    }
    public ngOnDestroy() {
        this._settingsStoreSubscription.unsubscribe();
        this._runtimeStoreSubscription.unsubscribe();
        this._formWrapper.cleanup();
    }
    public canDeactivate(): boolean {
        return !this.form.dirty;
    }
    private messageIfExists(name: string): boolean {
        let exists = !!this.settings.find((agent) => agent.name === name);
        if (exists) {
            this.dialogService.message(`An agent with the name ${name} already exists`);
            return true;
        }

        return false;
    }
}
