import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function passwordComplexityValidator(minLength = 8): ValidatorFn {
  const upper = /[A-Z]/;
  const lower = /[a-z]/;
  const digit = /[0-9]/;
  const symbol = /[^A-Za-z0-9]/;

  return (control: AbstractControl): ValidationErrors | null => {
    const v: string = control.value || '';
    if (!v) return null;

    const errors: ValidationErrors = {};
    if (v.length < minLength) errors['minLength'] = { required: minLength };
    if (!upper.test(v)) errors['uppercase'] = true;
    if (!lower.test(v)) errors['lowercase'] = true;
    if (!digit.test(v)) errors['digit'] = true;
    if (!symbol.test(v)) errors['symbol'] = true;

    return Object.keys(errors).length ? errors : null;
  };
}
