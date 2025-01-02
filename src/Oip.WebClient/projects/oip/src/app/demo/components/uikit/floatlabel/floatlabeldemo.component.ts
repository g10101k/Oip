import { Component, OnInit } from '@angular/core';
import { CountryService } from 'projects/oip/src/app/demo/service/country.service';
import { FormsModule } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { AutoComplete } from 'primeng/autocomplete';
import { DatePicker } from 'primeng/datepicker';
import { Chip } from 'primeng/chip';
import { InputMask } from 'primeng/inputmask';
import { InputNumber } from 'primeng/inputnumber';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelect } from 'primeng/multiselect';

@Component({
    templateUrl: './floatlabeldemo.component.html',
    imports: [FormsModule, InputText, AutoComplete, DatePicker, Chip, InputMask, InputNumber, InputGroup, InputGroupAddon, DropdownModule, MultiSelect]
})
export class FloatLabelDemoComponent implements OnInit {

  countries: any[] = [];

  cities: any[];

  filteredCountries: any[] = [];

  value1: any;

  value2: any;

  value3: any;

  value4: any;

  value5: any;

  value6: any;

  value7: any;

  value8: any;

  value9: any;

  value10: any;

  value11: any;

  value12: any;

  constructor(private countryService: CountryService) {
    this.cities = [
      { name: 'New York', code: 'NY' },
      { name: 'Rome', code: 'RM' },
      { name: 'London', code: 'LDN' },
      { name: 'Istanbul', code: 'IST' },
      { name: 'Paris', code: 'PRS' }
    ];
  }

  ngOnInit() {
    this.countryService.getCountries().then(countries => {
      this.countries = countries;
    });
  }

  searchCountry(event: any) {
    // in a real application, make a request to a remote url with the query and
    // return filtered results, for demo we filter at client side
    const filtered: any[] = [];
    const query = event.query;
    // tslint:disable-next-line:prefer-for-of
    for (let i = 0; i < this.countries.length; i++) {
      const country = this.countries[i];
      if (country.name.toLowerCase().indexOf(query.toLowerCase()) == 0) {
        filtered.push(country);
      }
    }

    this.filteredCountries = filtered;
  }
}
