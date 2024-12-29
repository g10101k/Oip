import { NgModule } from '@angular/core';
import { SharedLibComponent } from './shared-lib.component';
import { OtherComponent } from './other/other.component';

@NgModule({
  declarations: [SharedLibComponent, OtherComponent],
  imports: [],
  exports: [SharedLibComponent, OtherComponent]
})
export class SharedLibModule {
}
