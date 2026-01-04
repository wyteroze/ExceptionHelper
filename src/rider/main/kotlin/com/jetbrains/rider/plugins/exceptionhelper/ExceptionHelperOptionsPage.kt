package com.jetbrains.rider.plugins.exceptionhelper

import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class ExceptionHelperOptionsPage : SimpleOptionsPage(
    name = "Exception Helper",
    pageId = "ExceptionHelperOptionsPage"
) {
    override fun getId(): String {
        return "ExceptionHelperOptionsPage"
    }
}