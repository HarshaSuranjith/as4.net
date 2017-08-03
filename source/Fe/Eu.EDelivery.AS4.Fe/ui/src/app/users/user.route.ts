import { Routes } from '@angular/router';

import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { WrapperComponent } from './../common/wrapper.component';
import { UsersComponent } from './users/users.component';
import { CanDeactivateGuard } from './../common/candeactivate.guard';

export const ROUTES: Routes = [
    {
        path: '', component: WrapperComponent, children: [
            { path: 'users', component: UsersComponent, data: { title: 'User management', weight: 9999999999999999999999 }, canDeactivate: [CanDeactivateGuard] }
        ],
        canActivate: [MustBeAuthorizedGuard]
    }
];
