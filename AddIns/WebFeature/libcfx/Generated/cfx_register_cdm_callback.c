// Copyright (c) 2014-2017 Wolfgang Borgsmüller
// All rights reserved.
// 
// This software may be modified and distributed under the terms
// of the BSD license. See the License.txt file for details.

// Generated file. Do not edit.


// cef_register_cdm_callback

typedef struct _cfx_register_cdm_callback_t {
    cef_register_cdm_callback_t cef_register_cdm_callback;
    unsigned int ref_count;
    gc_handle_t gc_handle;
    int wrapper_kind;
    // managed callbacks
    void (CEF_CALLBACK *on_cdm_registration_complete)(gc_handle_t self, cef_cdm_registration_error_t result, char16 *error_message_str, int error_message_length);
} cfx_register_cdm_callback_t;

void CEF_CALLBACK _cfx_register_cdm_callback_add_ref(struct _cef_base_ref_counted_t* base) {
    InterlockedIncrement(&((cfx_register_cdm_callback_t*)base)->ref_count);
}
int CEF_CALLBACK _cfx_register_cdm_callback_release(struct _cef_base_ref_counted_t* base) {
    int count = InterlockedDecrement(&((cfx_register_cdm_callback_t*)base)->ref_count);
    if(count == 0) {
        if(((cfx_register_cdm_callback_t*)base)->wrapper_kind == 0) {
            cfx_gc_handle_switch(&((cfx_register_cdm_callback_t*)base)->gc_handle, GC_HANDLE_FREE);
        } else {
            cfx_gc_handle_switch(&((cfx_register_cdm_callback_t*)base)->gc_handle, GC_HANDLE_FREE | GC_HANDLE_REMOTE);
        }
        free(base);
        return 1;
    }
    return 0;
}
int CEF_CALLBACK _cfx_register_cdm_callback_has_one_ref(struct _cef_base_ref_counted_t* base) {
    return ((cfx_register_cdm_callback_t*)base)->ref_count == 1 ? 1 : 0;
}

static cfx_register_cdm_callback_t* cfx_register_cdm_callback_ctor(gc_handle_t gc_handle, int wrapper_kind) {
    cfx_register_cdm_callback_t* ptr = (cfx_register_cdm_callback_t*)calloc(1, sizeof(cfx_register_cdm_callback_t));
    if(!ptr) return 0;
    ptr->cef_register_cdm_callback.base.size = sizeof(cef_register_cdm_callback_t);
    ptr->cef_register_cdm_callback.base.add_ref = _cfx_register_cdm_callback_add_ref;
    ptr->cef_register_cdm_callback.base.release = _cfx_register_cdm_callback_release;
    ptr->cef_register_cdm_callback.base.has_one_ref = _cfx_register_cdm_callback_has_one_ref;
    ptr->ref_count = 1;
    ptr->gc_handle = gc_handle;
    ptr->wrapper_kind = wrapper_kind;
    return ptr;
}

// on_cdm_registration_complete

void CEF_CALLBACK cfx_register_cdm_callback_on_cdm_registration_complete(cef_register_cdm_callback_t* self, cef_cdm_registration_error_t result, const cef_string_t* error_message) {
    ((cfx_register_cdm_callback_t*)self)->on_cdm_registration_complete(((cfx_register_cdm_callback_t*)self)->gc_handle, result, error_message ? error_message->str : 0, error_message ? (int)error_message->length : 0);
}

static void cfx_register_cdm_callback_set_callback(cef_register_cdm_callback_t* self, int index, void* callback) {
    switch(index) {
    case 0:
        ((cfx_register_cdm_callback_t*)self)->on_cdm_registration_complete = (void (CEF_CALLBACK *)(gc_handle_t self, cef_cdm_registration_error_t result, char16 *error_message_str, int error_message_length))callback;
        self->on_cdm_registration_complete = callback ? cfx_register_cdm_callback_on_cdm_registration_complete : 0;
        break;
    }
}

