import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppConfigComponent } from "./app.config.component";

@NgModule({
    imports: [RouterModule.forChild([
        { path: '', component: AppConfigComponent }
    ])],
    exports: [RouterModule]
})
export class AppConfigRoutingModule { }
