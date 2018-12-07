import { ChangeDetectorRef, Component, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs/Subscription';

import { validatePassword } from '../../common/passwordValidator';
import { CanComponentDeactivate } from './../../common/candeactivate.guard';
import { DialogService } from './../../common/dialog.service';
import { ModalService } from './../../common/modal/modal.service';
import { User, UsersService } from './../users.service';

@Component({
  selector: 'as4-users',
  templateUrl: 'users.component.html'
})
export class UsersComponent implements OnDestroy, CanComponentDeactivate {
  public form: FormGroup;
  public users: User[];
  public currentUser: User | null;
  public isNew: boolean = false;
  public newName: string | null = null;
  private _subscription: Subscription;
  constructor(
    private _formBuilder: FormBuilder,
    private _userService: UsersService,
    private _dialogService: DialogService,
    private _modalService: ModalService,
    private _changeDetectorRef: ChangeDetectorRef
  ) {
    this.setForm(null);
    this.reload();
  }
  public ngOnDestroy() {
    if (!!this._subscription) {
      this._subscription.unsubscribe();
    }
  }
  public canDeactivate(): boolean {
    return !this.form.dirty;
  }
  public selectUser(user: User | null) {
    const change = () => {
      this.isNew = false;
      this.currentUser = user;
      this.setForm(this.currentUser);
      if (!!!this.currentUser) {
        this.form.disable();
      }
    };
    if (this.form.dirty) {
      this._dialogService
        .confirm(
          `You have unsaved changes, are you sure you want to continue ?`,
          'Delete user'
        )
        .filter((result) => result)
        .subscribe(() => change());
      return;
    }
    change();
  }
  public add() {
    this._modalService
      .show('new-user')
      .filter((result) => result)
      .subscribe((result) => {
        this.isNew = true;
        this.currentUser = new User();
        this.currentUser.name = this.newName!;
        this.users = [...this.users, this.currentUser];
        this.setForm(this.currentUser);
        this.newName = null;
        this.form.markAsDirty();
      });
  }
  public save() {
    if (this.isNew) {
      this._userService.create(this.form.value).subscribe(() => this.reload());
    } else {
      this._userService.update(this.form.value).subscribe(() => this.reload());
    }
  }
  public reset() {
    if (this.isNew) {
      this.users = this.users.filter((user) => user !== this.currentUser);
    }
    this.currentUser = null;
    this.setForm(this.currentUser);
    this.form.disable();
  }
  public delete() {
    this._dialogService
      .confirm(`Are you sure you want to delete ${this.currentUser!.name}`)
      .filter((result) => result)
      .switchMap(() => this._userService.delete(this.currentUser!.name))
      .subscribe(() => {
        this.currentUser = null;
        this.reload();
      });
  }
  private reload() {
    if (!!this.currentUser) {
      this.currentUser = this.form.value;
    }
    this.setForm(this.currentUser);
    this.form.markAsPristine();
    this._userService.get().subscribe((result) => {
      this.users = result;
    });
  }
  private setForm(user: User | null) {
    this.form = this._formBuilder.group({
      name: [!!!user ? '' : user.name, Validators.required],
      roles: [
        !!user && !!user.roles && user.roles.length > 0 ? user.roles[0] : null,
        Validators.required
      ],
      password: ['', this.isNew ? Validators.required : validatePassword]
    });
    if (!!!user) {
      this.form.disable();
    } else {
      this.form.enable();
    }
  }
}
