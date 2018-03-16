import 'rxjs/add/operator/combineLatest';

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';

import { SettingsAgent } from '../../api/SettingsAgent';
import { SettingsAgentForm } from '../../api/SettingsAgentForm';
import { RuntimeStore } from '../runtime.store';
import { SettingsService } from '../settings.service';
import { SettingsStore } from '../settings.store';
import { ItemType } from './../../api/ItemType';
import { CanComponentDeactivate } from './../../common/candeactivate.guard';
import { DialogService } from './../../common/dialog.service';
import { FormBuilderExtended, FormWrapper } from './../../common/form.service';
import { ModalService } from './../../common/modal/modal.service';
import { Transformer } from '../../api';

@Component({
    selector: 'as4-agent-settings',
    templateUrl: './agent.component.html'
})
export class AgentSettingsComponent implements OnDestroy, CanComponentDeactivate, OnInit {
    public settings: SettingsAgent[] = new Array<SettingsAgent>();
    public collapsed: boolean = true;
    public showWarning: boolean = false;

    public get currentAgent(): SettingsAgent | undefined {
        return this._currentAgent;
    }
    public set currentAgent(agent: SettingsAgent | undefined) {
        this._currentAgent = agent;
    }
    public transformers: ItemType[];
    public isNewMode: boolean = false;
    public newName: string;
    public actionType: string | number = 0;

    public form: FormGroup;
    @Input() public title: string;
    @Input() public agent: string;
    @Input() public beType: number;

    private _currentAgent: SettingsAgent | undefined;
    private _subscription: Subscription;
    private _formWrapper: FormWrapper;

    constructor(private settingsStore: SettingsStore, private settingsService: SettingsService,
        // tslint:disable-next-line:align
        private activatedRoute: ActivatedRoute, private runtimeStore: RuntimeStore,
        private dialogService: DialogService, private modalService: ModalService,
        private formBuilder: FormBuilderExtended) {
        this._formWrapper = this.formBuilder.get();
        this.form = SettingsAgentForm.getForm(this._formWrapper, undefined).build();
        this._formWrapper.disable();
        if (!!this.activatedRoute.snapshot.data['type']) {
            this.title = `${this.activatedRoute.snapshot.data['title']} agent`;
            this.collapsed = false;
            this.agent = this.activatedRoute.snapshot.data['type'];
            this.beType = this.activatedRoute.snapshot.data['betype'];
            this.showWarning = this.activatedRoute.snapshot.data['showwarning'];
        }
    }
    public ngOnInit() {
        let settingsStoreSelector = this
            .settingsStore
            .changes
            .filter((result) => !!result && !!result.Settings && !!result.Settings.agents[this.agent])
            .map((result) => result.Settings.agents[this.agent] as SettingsAgent[]);
        const defaultTransformer = this.settingsService.getDefaultAgentTransformer(this.beType);
        let sub = Observable
            .combineLatest(settingsStoreSelector, defaultTransformer)
            .filter(([agents, transformers]) => !!agents && agents.length > 0 && !!transformers && !!transformers.defaultTransformer)
            .subscribe(([agents, transformers]) => {
                this.settings = agents;
                if (!!this.currentAgent) {
                    this.currentAgent = agents.find((agt) => agt.name === this.currentAgent!.name);
                } else if (!!agents && agents.length > 0) {
                    this.currentAgent = agents[0];
                }
                this.transformers = [transformers.defaultTransformer].concat(transformers.otherTransformers);
                this.form = SettingsAgentForm.getForm(this._formWrapper, this.currentAgent).build(!!!this.currentAgent);
            });
        this._subscription = sub;
    }
    public addAgent() {
        this.modalService
            .show('new-agent')
            .filter((result) => result)
            .subscribe(() => {
                if (!!this.newName) {
                    this.isNewMode = true;
                    if (this.messageIfExists(this.newName)) {
                        return;
                    }

                    const setupCurrent = (agent) => {
                        this.currentAgent = agent;
                        this.currentAgent!.name = this.newName;
                        this.settingsStore.addAgent(this.agent, this.currentAgent!);
                        this.form = SettingsAgentForm.getForm(this._formWrapper, this.currentAgent).build(false);
                        this.form.markAsDirty();
                        setTimeout(() => this.form.enable());
                    };

                    let newAgent: SettingsAgent;
                    if (+this.actionType !== -1) {
                        newAgent = <SettingsAgent>Object.assign({},
                            this.settings.find((agt) => agt.name === this.actionType));
                    } else {
                        newAgent = new SettingsAgent();
                        const defaultTransformer = this.settingsService.getDefaultAgentTransformer(this.beType);
                        const defaultSteps = this.settingsService.getDefaultAgentSteps(this.beType);
                        Observable
                            .combineLatest(defaultTransformer, defaultSteps)
                            .subscribe(([transformer, steps]) => {
                                newAgent.stepConfiguration = steps;
                                newAgent.transformer = new Transformer();
                                newAgent.transformer.type = transformer.defaultTransformer.technicalName;
                                setupCurrent(newAgent);

                                this.transformers = [transformer.defaultTransformer].concat(transformer.otherTransformers);
                            });
                        return;
                    }

                    setupCurrent(newAgent);
                }
            });
    }
    public selectAgent(selectedAgent: string | undefined = undefined): boolean {
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
                    }
                    select();
                });

            return false;
        }

        select();
        return true;
    }
    public save() {
        if (!!!this.currentAgent) {
            return;
        }

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

            this.dialogService.message(`Settings will only be applied after restarting the runtime!`, 'Attention');
        });
    }
    public reset() {
        if (this.isNewMode && !!this.currentAgent) {
            this.settingsStore.deleteAgent(this.agent, this.currentAgent);
            this.currentAgent = undefined;
        }
        this.isNewMode = false;
        this.form = SettingsAgentForm.getForm(this._formWrapper, this.currentAgent).build(!!!this.currentAgent);
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
        if (!!!this.currentAgent) {
            return;
        }
        this.dialogService
            .confirm('Are you sure you want to delete the agent', 'Delete agent')
            .filter((result) => result)
            .subscribe(() => {
                if (this.isNewMode) {
                    this.reset();
                    return;
                }

                this.settingsService.deleteAgent(this.currentAgent!, this.agent);
            });
    }
    public ngOnDestroy() {
        if (!!this._subscription) {
            this._subscription.unsubscribe();
        }
        this._formWrapper.cleanup();
    }
    public canDeactivate(): boolean {
        return !this.form.dirty;
    }
    private messageIfExists(name: string): boolean {
        let exists = !!this.settings.find((agent) => agent.name.toLocaleLowerCase() === name.toLocaleLowerCase());
        if (exists) {
            this.dialogService.message(`An agent with the name ${name} already exists`);
            return true;
        }

        return false;
    }
}
